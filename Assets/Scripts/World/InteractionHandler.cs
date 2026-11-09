using UnityEngine;

/// <summary>
/// Xử lý tương tác với tất cả điểm trong thế giới.
/// Lắng nghe InteractionZone.OnActivated và điều phối logic game tương ứng.
/// </summary>
public class InteractionHandler : MonoBehaviour
{
    private GameState _s => GameManager.Instance?.State;

    // ─────────────────────────────────────────────────────────────────────────
    void OnEnable()  => InteractionZone.OnActivated += Handle;
    void OnDisable() => InteractionZone.OnActivated -= Handle;

    // ─────────────────────────────────────────────────────────────────────────
    void Handle(InteractionZone zone)
    {
        if (_s == null) return;

        switch (zone.interactionId)
        {
            // ── PHÒNG TRỌ ───────────────────────────────────────────────
            case "bed":
                HandleBed();
                break;

            case "laptop":
                HandleLaptop();
                break;

            // ── QUÁN COFFEE ─────────────────────────────────────────────
            case "manager":
                HandleManager();
                break;

            case "tray":
                HandleTray();
                break;

            case "customer":
                HandleCustomer();
                break;

            // ── TRƯỜNG ──────────────────────────────────────────────────
            case "friend":
                HandleFriend();
                break;

            case "seat":
                HandleSeat();
                break;

            case "teacher":
                HandleTeacher();
                break;

            // ── XE MÁY ──────────────────────────────────────────────────
            case "bike":
                HandleBike();
                break;

            // ── PHỐ ─────────────────────────────────────────────────────
            case "food":
                HandleFood();
                break;

            case "help":
                HandleEncounter("help");
                break;

            case "wallet":
                HandleEncounter("wallet");
                break;

            // ── GARAGE ──────────────────────────────────────────────────
            case "repair":
                HandleRepair();
                break;

            case "bench":
                HandleBench();
                break;

            default:
                Debug.Log($"[Interaction] Chưa xử lý: {zone.interactionId}");
                break;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CÁC HÀM XỬ LÝ TỪNG ĐIỂM
    // ─────────────────────────────────────────────────────────────────────────

    void HandleBed()
    {
        // Nếu chưa chọn hoạt động chiều → mở menu lựa chọn
        if (string.IsNullOrEmpty(_s.choice))
        {
            ShowChoiceMenu();
            return;
        }
        // Nghỉ ngơi / ngủ
        var result = LifeData.StartSleep(_s);
        if (result.ok)
        {
            HUDController.FindAnyObjectByType<HUDController>()
                ?.ShowNotice("Bắt đầu ngủ. Đồng hồ vẫn chạy.");
        }
        else
        {
            HUDController.FindAnyObjectByType<HUDController>()
                ?.ShowNotice(result.reason);
        }
    }

    void HandleLaptop()
    {
        var dlg = new DialogueData
        {
            title   = "Góc học tập",
            speaker = "LAPTOP · PHÒNG TRỌ",
            quote   = "Bạn mở laptop. Có thể học bài, lên kế hoạch chi tiêu hoặc xem tin tức.",
            options = new System.Collections.Generic.List<DialogueOption>
            {
                new DialogueOption { label = "Học bài (30 phút)", description = "+8 học tập · −5 năng lượng" },
                new DialogueOption { label = "Lên kế hoạch chi tiêu", description = "+3 học tập" },
                new DialogueOption { label = "Đóng laptop", description = "Không làm gì" }
            }
        };

        DialogueUI.Instance?.Show(dlg, idx =>
        {
            if (idx == 0) { _s.ApplyDelta(new StatDelta { study = 8, energy = -5 }, "Học laptop 30 phút"); TimeManager.Instance?.AdvanceOneMinute(); }
            else if (idx == 1) { _s.ApplyDelta(new StatDelta { study = 3 }, "Lên kế hoạch chi tiêu"); }
        });
    }

    void HandleManager()
    {
        if (!string.IsNullOrEmpty(_s.choice)) return;

        var dlg = BuildChoiceDialogue(
            "ANH MINH · QUẢN LÝ QUÁN",
            "\"Hôm nay bạn rảnh không? Mình cần người phụ ca chiều. Lương 180.000đ.\"",
            StoryData.CHOICES["WORK"]);

        DialogueUI.Instance?.Show(dlg, idx =>
        {
            if (idx == 0)  // Nhận ca
            {
                StoryData.ApplyChoice(_s, "WORK");
                _s.task = "serve";
                DialogueUI.Instance?.ShowReceipt(
                    StoryData.CHOICES["WORK"].receipt,
                    StoryData.CHOICES["WORK"].lesson);
            }
        });
    }

    void HandleTray()
    {
        if (_s.task != "serve" || _s.stage != 0) return;
        _s.stage = 1;
        HUDController.FindAnyObjectByType<HUDController>()
            ?.ShowNotice("Nhận khay thành công! Giao cho khách bàn 02.");
    }

    void HandleCustomer()
    {
        if (_s.task != "serve" || _s.stage != 1) return;
        _s.stage = 0;
        _s.task  = null;
        _s.ApplyDelta(new StatDelta { trust = 2 }, "Phục vụ khách thành công");
        HUDController.FindAnyObjectByType<HUDController>()
            ?.ShowNotice("Giao cà phê thành công! +2 quan hệ.");
    }

    void HandleFriend()
    {
        if (!string.IsNullOrEmpty(_s.choice)) return;

        var dlg = BuildChoiceDialogue(
            "LINH · BẠN CÙNG NHÓM",
            "\"Nhóm đang họp chuẩn bị bài thuyết trình. Bạn đến cùng không?\"",
            StoryData.CHOICES["MEET"]);

        DialogueUI.Instance?.Show(dlg, idx =>
        {
            if (idx == 0)
            {
                StoryData.ApplyChoice(_s, "MEET");
                _s.task = "meet";
                DialogueUI.Instance?.ShowReceipt(
                    StoryData.CHOICES["MEET"].receipt,
                    StoryData.CHOICES["MEET"].lesson);
            }
        });
    }

    void HandleSeat()
    {
        if (_s.task != "meet") return;
        _s.task  = null;
        _s.stage = 0;
        _s.ApplyDelta(new StatDelta { study = 5 }, "Hoàn thành họp nhóm");
        HUDController.FindAnyObjectByType<HUDController>()
            ?.ShowNotice("Đã hoàn thành họp nhóm! +5 học tập.");
    }

    void HandleTeacher()
    {
        var dlg = new DialogueData
        {
            title   = "Thầy An",
            speaker = "THẦY AN · GIẢNG VIÊN",
            quote   = "\"Nhóm bạn chuẩn bị được không? Bài thuyết trình tuần này quan trọng lắm. Bạn cần tư vấn thêm không?\"",
            options = new System.Collections.Generic.List<DialogueOption>
            {
                new DialogueOption { label = "Hỏi về bài tập nhóm", description = "+5 học tập · 10 phút" },
                new DialogueOption { label = "Thôi để tự làm",        description = "Tiếp tục" }
            }
        };

        DialogueUI.Instance?.Show(dlg, idx =>
        {
            if (idx == 0) _s.ApplyDelta(new StatDelta { study = 5 }, "Hỏi thầy về bài tập");
        });
    }

    void HandleBike()
    {
        if (PlayerController.Instance == null) return;

        if (PlayerController.Instance.IsOnBike)
            PlayerController.Instance.GetOffBike();
        else
        {
            if (_s.bikeCondition <= 0)
            {
                HUDController.FindAnyObjectByType<HUDController>()
                    ?.ShowNotice("Xe hỏng! Cần đến Garage 08 sửa xe trước.");
                return;
            }
            PlayerController.Instance.GetOnBike(new Vector3(-17f, 0f, -6.6f));
        }
    }

    void HandleFood()
    {
        int cost = 25_000;
        var (ok, reason) = LifeData.EatMeal(_s, cost, energyGain: 20);
        string msg = ok ? $"Đã mua bánh mì (−{cost:N0}đ). +20 năng lượng." : reason;
        HUDController.FindAnyObjectByType<HUDController>()?.ShowNotice(msg);
    }

    void HandleEncounter(string id)
    {
        if (_s.flags.ContainsKey(id)) return;
        if (!StoryData.ENCOUNTERS.TryGetValue(id, out var enc)) return;

        var opts = new System.Collections.Generic.List<DialogueOption>();
        foreach (var o in enc.options)
            opts.Add(new DialogueOption { label = o.label, description = o.description });

        var dlg = new DialogueData
        {
            title   = enc.title,
            speaker = enc.speaker,
            quote   = enc.quote,
            options = opts
        };

        DialogueUI.Instance?.Show(dlg, idx =>
        {
            StoryData.ApplyEncounter(_s, id, enc.options[idx].id);
        });
    }

    void HandleRepair()
    {
        if (_s.bikeCondition >= 100)
        {
            HUDController.FindAnyObjectByType<HUDController>()
                ?.ShowNotice("Xe vẫn còn tốt, không cần sửa.");
            return;
        }
        int damage = 100 - _s.bikeCondition;
        int cost   = damage * 2500;

        var dlg = new DialogueData
        {
            title   = "Chú Tâm · Sửa xe",
            speaker = "GARAGE 08 · SỬA XE",
            quote   = $"\"Xe bạn hỏng {damage} điểm. Sửa hết {cost:N0}đ. Có sửa không?\"",
            options = new System.Collections.Generic.List<DialogueOption>
            {
                new DialogueOption { label = "Sửa xe ngay",          description = $"−{cost:N0}đ · Xe 100%" },
                new DialogueOption { label = "Thôi, để sau",          description = "Rủi ro hỏng thêm" }
            }
        };

        DialogueUI.Instance?.Show(dlg, idx =>
        {
            if (idx == 0 && _s.money >= cost)
            {
                _s.ApplyDelta(new StatDelta { money = -cost }, "Sửa xe");
                _s.bikeCondition = 100;
                HUDController.FindAnyObjectByType<HUDController>()
                    ?.ShowNotice("Xe đã được sửa! 100%.");
            }
            else if (idx == 0)
            {
                HUDController.FindAnyObjectByType<HUDController>()
                    ?.ShowNotice("Không đủ tiền sửa xe.");
            }
        });
    }

    void HandleBench()
    {
        _s.ApplyDelta(new StatDelta { energy = 5 }, "Nghỉ chân trên ghế");
        HUDController.FindAnyObjectByType<HUDController>()?.ShowNotice("Nghỉ chân. +5 năng lượng.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // MENU LỰA CHỌN CHIỀU (MEET / WORK / REST)
    // ─────────────────────────────────────────────────────────────────────────
    void ShowChoiceMenu()
    {
        var dlg = new DialogueData
        {
            title   = "Quyết định chiều nay",
            speaker = "PHÒNG TRỌ 08 · THANH XUÂN",
            quote   = "Chiều nay bạn muốn làm gì? Mỗi lựa chọn đều có hệ quả.",
            options = new System.Collections.Generic.List<DialogueOption>
            {
                new DialogueOption
                {
                    label       = StoryData.CHOICES["MEET"].name,
                    description = "−60.000đ · +12 học tập · +5 quan hệ · −10 năng lượng"
                },
                new DialogueOption
                {
                    label       = StoryData.CHOICES["WORK"].name,
                    description = "+180.000đ · −25 năng lượng · −5 quan hệ"
                },
                new DialogueOption
                {
                    label       = StoryData.CHOICES["REST"].name,
                    description = "−40.000đ · +20 năng lượng"
                }
            }
        };

        string[] keys = { "MEET", "WORK", "REST" };
        DialogueUI.Instance?.Show(dlg, idx =>
        {
            StoryData.ApplyChoice(_s, keys[idx]);
            var c = StoryData.CHOICES[keys[idx]];
            DialogueUI.Instance?.ShowReceipt(c.receipt, c.lesson);
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    DialogueData BuildChoiceDialogue(string speaker, string quote, Choice c)
    {
        return new DialogueData
        {
            title   = c.name,
            speaker = speaker,
            quote   = quote,
            options = new System.Collections.Generic.List<DialogueOption>
            {
                new DialogueOption { label = c.name,           description = c.receipt },
                new DialogueOption { label = "Không, cảm ơn.", description = "Từ chối" }
            }
        };
    }
}

