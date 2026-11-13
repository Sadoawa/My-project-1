"""Bake solid Vietnamese sign outlines from the project's licensed Roboto Bold.

Requires fontTools, shapely, mapbox-earcut and numpy. Dimensions in metres.
This produces geometry data consumed by the native Unity editor builder.
"""
import json
from pathlib import Path
import numpy as np
import mapbox_earcut
from fontTools.ttLib import TTFont
from fontTools.pens.basePen import BasePen
from shapely.geometry import Polygon, GeometryCollection
from shapely.geometry.polygon import orient
from shapely.affinity import translate, scale

ROOT = Path(__file__).resolve().parents[2]
FONT = ROOT / 'Assets/TextMesh Pro/Examples & Extras/Fonts/Roboto-Bold.ttf'
TEXT = 'ĐẠI HỌC KINH TẾ TP. HỒ CHÍ MINH'
font = TTFont(FONT)
glyphs = font.getGlyphSet()
cmap = font.getBestCmap()
cap = font['OS/2'].sCapHeight
factor = 1.2 / cap

class Contours(BasePen):
    def __init__(self):
        super().__init__(glyphs)
        self.paths = []
    def _moveTo(self, p): self.path = [p]
    def _lineTo(self, p): self.path.append(p)
    def _qCurveToOne(self, c, p):
        a = self.path[-1]
        for t in np.linspace(0, 1, 17)[1:]:
            self.path.append(tuple((1-t)**2*a[i]+2*(1-t)*t*c[i]+t*t*p[i] for i in range(2)))
    def _curveToOne(self, b, c, p):
        a = self.path[-1]
        for t in np.linspace(0, 1, 21)[1:]:
            self.path.append(tuple((1-t)**3*a[i]+3*(1-t)**2*t*b[i]+3*(1-t)*t*t*c[i]+t**3*p[i] for i in range(2)))
    def _closePath(self): self.paths.append(self.path)
    def _endPath(self): self._closePath()

def shape(char):
    pen = Contours(); glyphs[cmap[ord(char)]].draw(pen)
    geom = GeometryCollection()
    for ring in pen.paths:
        if len(ring) >= 3:
            p = Polygon(ring)
            if not p.is_valid: p = p.buffer(0)
            geom = geom.symmetric_difference(p)
    return scale(geom, xfact=factor, yfact=factor, origin=(0,0)).buffer(.003, join_style=2)

def extrusion(geom, back, front):
    vertices, triangles = [], []
    def tri(a,b,c):
        offset=len(vertices)//3
        vertices.extend([round(v,6) for p in (a,b,c) for v in p])
        triangles.extend([offset,offset+1,offset+2])
    for polygon in ([geom] if geom.geom_type=='Polygon' else geom.geoms):
        polygon=orient(polygon,sign=1)
        rings=[list(polygon.exterior.coords)[:-1]]+[list(r.coords)[:-1] for r in polygon.interiors]
        points=np.asarray([p for r in rings for p in r],dtype=np.float64)
        ends=np.cumsum([len(r) for r in rings],dtype=np.uint32)
        indices=mapbox_earcut.triangulate_float64(points,ends).reshape(-1,3)
        for idx in indices:
            a,b,c=[tuple(points[i]) for i in idx]
            cross=(b[0]-a[0])*(c[1]-a[1])-(b[1]-a[1])*(c[0]-a[0])
            if cross<0: b,c=c,b
            tri((*a,back),(*b,back),(*c,back))
            tri((*a,front),(*c,front),(*b,front))
        for ring in rings:
            for a,b in zip(ring,ring[1:]+ring[:1]):
                tri((*a,back),(*a,front),(*b,front))
                tri((*a,back),(*b,front),(*b,back))
    return dict(vertices=vertices,triangles=triangles)

cursor=0
entries=[]
for char in TEXT:
    advance=font['hmtx'].metrics[cmap[ord(char)]][0]*factor+.085
    if char != ' ':
        geom=translate(shape(char),xoff=cursor)
        entries.append((char,geom))
    cursor+=advance
minimum=min(g.bounds[0] for _,g in entries)
maximum=max(g.bounds[2] for _,g in entries)
width=maximum-minimum
# Match the broad freestanding sign in the supplied panorama without stretching glyphs.
fit=min(1,32/width)
merged={'blue':{'vertices':[],'triangles':[]},'white':{'vertices':[],'triangles':[]}}
supports=[]
for char,geom in entries:
    geom=scale(translate(geom,xoff=-minimum),xfact=fit,yfact=fit,origin=(0,0))
    for name,g,back,front in [('blue',geom,-.145,-.155),('white',geom.buffer(.028,join_style=2),0,-.14)]:
        mesh=extrusion(g,back,front); out=merged[name]; offset=len(out['vertices'])//3
        out['vertices'].extend(mesh['vertices']);out['triangles'].extend(i+offset for i in mesh['triangles'])
    # Two discreet legs for broad glyphs; dots/diacritics are supported by the letter's rear fixing.
    x0,y0,x1,y1=geom.bounds
    if char!='.': supports.append((x0+x1)/2)
data=dict(text=TEXT,width=width*fit,capHeight=1.2*fit,outlineWidth=.028,bodyDepth=.14,supportX=supports,**merged)
out=ROOT/'Assets/UEHCampusB/Fonts/B1RaisedSignGeometry.json'
out.write_text(json.dumps(data,separators=(',',':'),ensure_ascii=False),encoding='utf-8')
print(json.dumps({k:data[k] for k in ['text','width','capHeight','outlineWidth','bodyDepth']},ensure_ascii=True))
