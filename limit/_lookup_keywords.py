# -*- coding: utf-8 -*-
import re
import pathlib

base = pathlib.Path(r"D:\software\steam\steamapps\common\LBoL\LBoL_Data\StreamingAssets\Localization\zh-Hans")
files = {
    "StatusEffect": base / "StatusEffect.yaml",
    "Keyword": base / "Keyword.yaml",
    "Exhibit": base / "Exhibit.yaml",
    "Card": base / "Card.yaml",
}
names = [
    "护身符", "滑溜", "固有", "灵力", "护盾", "锁定", "格挡", "闪避", "中毒", "虚弱",
    "天衣无缝", "火力", "使魔", "纯化", "恩惠", "狼毛", "花果子念报", "采购物资",
    "戏水", "新闻捏造", "璀璨星光", "亡灵的送行提灯", "蓬莱玉枝", "空白卡牌",
    "毘沙门天的宝塔", "寻龙尺", "闪亮的灯泡", "守矢御币", "赛钱箱", "一次性",
    "永夜返", "额外回合", "无敌", "临时火力", "临时灵力",
]
parsed = {}
idmap = {}
for kind, path in files.items():
    text = path.read_text(encoding="utf-8")
    current = None
    for line in text.splitlines():
        m = re.match(r"^([A-Za-z0-9_]+):\s*$", line)
        if m:
            current = m.group(1)
            continue
        m = re.match(r'\s+Name:\s*"(.*)"\s*$', line)
        if m and current:
            parsed.setdefault(m.group(1), []).append((kind, current))
            idmap[(kind, current)] = m.group(1)

for n in names:
    hits = parsed.get(n, [])
    if hits:
        print(f"{n} => " + ", ".join(f"{k}:{i}" for k, i in hits))
    else:
        fuzzy = [f"{name}->{k}:{i}" for name, v in parsed.items() if n in name for k, i in v]
        print("NOT FOUND:", n, ("| fuzzy: " + " ; ".join(fuzzy[:8]) if fuzzy else ""))

print("--- reverse id lookup ---")
ids = [
    "Invincible", "Grace", "Amulet", "LockedOn", "Spirit", "Firepower",
    "TempFirepower", "TempSpirit", "Poison", "Weak", "Graze", "Servant",
    "ExtraTurn", "SuperExtraTurn", "Block", "Shield", "Purify", "Weakness",
]
for i in ids:
    for kind in files:
        if (kind, i) in idmap:
            print(f"{kind}:{i} => {idmap[(kind, i)]}")
