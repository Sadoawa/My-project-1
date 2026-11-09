"""Generate solid uppercase glyphs from the project's licensed Liberation Sans.

Run from the project root with Python and fontTools. Unity consumes the saved
JSON, so normal scene editing/rebuilding does not require Python or fontTools.
Scanline trapezoids tessellate filled outlines using the even-odd rule, retaining
holes in letters such as O, B and A. Front Z=-1, back Z=0; Unity scales the depth.
"""
from pathlib import Path
import json
import math
from fontTools.ttLib import TTFont
from fontTools.pens.basePen import BasePen


class FlattenPen(BasePen):
    def __init__(self, glyphs, scale):
        super().__init__(glyphs)
        self.scale = scale
        self.contours = []
        self.current = []

    def _point(self, point):
        p = (point[0] * self.scale, point[1] * self.scale)
        if not self.current or math.dist(p, self.current[-1]) > 1e-7:
            self.current.append(p)

    def _moveTo(self, point):
        self.current = []
        self._point(point)

    def _lineTo(self, point):
        self._point(point)

    def _qCurveToOne(self, control, end):
        start = self._getCurrentPoint()
        steps = max(4, math.ceil((math.dist(start, control) + math.dist(control, end)) * self.scale * 24))
        for i in range(1, steps + 1):
            t = i / steps
            self._point(tuple((1-t)**2*start[k] + 2*(1-t)*t*control[k] + t*t*end[k] for k in (0, 1)))

    def _curveToOne(self, a, b, end):
        start = self._getCurrentPoint()
        for i in range(1, 25):
            t = i / 24
            self._point(tuple((1-t)**3*start[k] + 3*(1-t)**2*t*a[k] + 3*(1-t)*t*t*b[k] + t**3*end[k] for k in (0, 1)))

    def _closePath(self):
        if len(self.current) > 2:
            if math.dist(self.current[0], self.current[-1]) < 1e-7:
                self.current.pop()
            self.contours.append(self.current)
        self.current = []

    def _endPath(self):
        self._closePath()


def solid(contours):
    edges = [(a, b) for ring in contours for a, b in zip(ring, ring[1:] + ring[:1])]
    levels = sorted(set(round(p[1], 8) for ring in contours for p in ring))
    vertices, triangles = [], []

    def triangle(a, b, c):
        cross = ((b[1]-a[1])*(c[2]-a[2])-(b[2]-a[2])*(c[1]-a[1]),
                 (b[2]-a[2])*(c[0]-a[0])-(b[0]-a[0])*(c[2]-a[2]),
                 (b[0]-a[0])*(c[1]-a[1])-(b[1]-a[1])*(c[0]-a[0]))
        if sum(v*v for v in cross) < 1e-18:
            return
        start = len(vertices) // 3
        for point in (a, b, c):
            vertices.extend(round(value, 7) for value in point)
        triangles.extend((start, start+1, start+2))

    def x_at(edge, y):
        a, b = edge
        return a[0] + (y-a[1])*(b[0]-a[0])/(b[1]-a[1])

    for low, high in zip(levels, levels[1:]):
        mid = (low + high) / 2
        active = sorted((edge for edge in edges if min(edge[0][1], edge[1][1]) < mid < max(edge[0][1], edge[1][1])), key=lambda edge: x_at(edge, mid))
        if len(active) % 2:
            raise ValueError('Unpaired font contour intersections')
        for left, right in zip(active[::2], active[1::2]):
            points = [(x_at(left, low), low), (x_at(right, low), low),
                      (x_at(right, high), high), (x_at(left, high), high)]
            for depth, reverse in ((-1, True), (0, False)):
                p = [(x, y, depth) for x, y in points]
                for a, b, c in ((0, 1, 2), (0, 2, 3)):
                    triangle(p[a], p[c] if reverse else p[b], p[b] if reverse else p[c])

    def inside(x, y):
        return sum(1 for edge in edges if min(edge[0][1], edge[1][1]) < y < max(edge[0][1], edge[1][1]) and x_at(edge, y) > x) % 2 == 1

    for a, b in edges:
        dx, dy = b[0]-a[0], b[1]-a[1]
        length = math.hypot(dx, dy)
        if length < 1e-8:
            continue
        # The quad below faces right of the directed contour edge.
        left_inside = inside((a[0]+b[0])/2-dy/length*1e-5, (a[1]+b[1])/2+dx/length*1e-5)
        p = [(a[0], a[1], -1), (b[0], b[1], -1), (b[0], b[1], 0), (a[0], a[1], 0)]
        for i, j, k in ((0, 1, 2), (0, 2, 3)):
            triangle(p[i], p[j] if left_inside else p[k], p[k] if left_inside else p[j])
    return vertices, triangles


def main():
    root = Path.cwd()
    font = TTFont(root / 'Assets/UEHCampusB/Fonts/LiberationSans.ttf')
    glyphs = font.getGlyphSet()
    cmap = font.getBestCmap()
    from fontTools.pens.boundsPen import BoundsPen
    bounds = BoundsPen(glyphs)
    glyphs[cmap[ord('H')]].draw(bounds)
    scale = 1 / bounds.bounds[3]
    result = []
    for letter in 'ABCDEFGHIJKLMNOPQRSTUVWXYZ':
        glyph_name = cmap[ord(letter)]
        pen = FlattenPen(glyphs, scale)
        glyphs[glyph_name].draw(pen)
        vertices, triangles = solid(pen.contours)
        result.append(dict(letter=letter, advance=font['hmtx'][glyph_name][0]*scale,
                           vertices=vertices, triangles=triangles))
    target = root / 'Assets/UEHCampusB/Fonts/UppercaseSolidGlyphs.json'
    target.write_text(json.dumps(dict(glyphs=result), separators=(',', ':')), encoding='utf-8')
    print(f'Saved {len(result)} solid glyphs to {target.relative_to(root)}')


if __name__ == '__main__':
    main()
