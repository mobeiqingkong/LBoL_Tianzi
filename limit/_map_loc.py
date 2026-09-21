# -*- coding: utf-8 -*-
import re, pathlib

base = pathlib.Path(r"D:\software\steam\steamapps\common\LBoL\LBoL_Data\StreamingAssets\Localization")

def parse(path):
    text = path.read_text(encoding="utf-8")
    entries = {}
    cur = None
    for line in text.splitlines():
        m = re.match(r'^(\w+):\s*$', line)
        if m:
            cur = m.group(1)
            entries[cur] = {}
            continue
        if cur is None:
            continue
        m2 = re.match(r'^  (\w+):\s*(.*)$', line)
        if m2:
            key, val = m2.group(1), m2.group(2)
            if val == '|-':
                entries[cur][key] = []
                entries[cur]['_ml'] = key
            else:
                entries[cur][key] = val.strip('"')
                entries[cur].pop('_ml', None)
            continue
        if cur in entries and '_ml' in entries[cur]:
            k = entries[cur]['_ml']
            if isinstance(entries[cur][k], list):
                entries[cur][k].append(line.strip())
    # flatten multiline
    for e in entries.values():
        e.pop('_ml', None)
        for k,v in list(e.items()):
            if isinstance(v, list):
                e[k] = "\n".join(v)
    return entries

def name_map(zh_path, en_path):
    zh = parse(zh_path)
    en = parse(en_path)
    out = {}
    for id_, z in zh.items():
        zn = z.get("Name")
        ename = en.get(id_, {}).get("Name")
        if zn and ename:
            out[zn] = (id_, ename)
    return out

se = name_map(base/"zh-Hans"/"StatusEffect.yaml", base/"en"/"StatusEffect.yaml")
ex = name_map(base/"zh-Hans"/"Exhibit.yaml", base/"en"/"Exhibit.yaml")
cd = name_map(base/"zh-Hans"/"Card.yaml", base/"en"/"Card.yaml")

wanted = [
  "护身符","滑溜","固有","亡灵的送行提灯","灵力","护盾","锁定","狼毛","《花果子念报》",
  "中毒","虚弱","闪避","聚能","采购物资","地狱之门","戏水","新闻捏造",
  "格挡","天衣无缝","胆小","白洞","纯化","火力","蓬莱玉枝","空白卡牌",
  "迷你隙间","毘沙门天的宝塔","寻龙尺","闪亮的灯泡","守矢御币","赛钱箱",
  "「永夜返」","不灭","使魔","禁止","暂失火力","暂失灵力","璀璨星光","一次性","难以灭杀",
  "厄运","花果子念报","送行提灯","永夜返"
]

print("=== LOOKUP ===")
for t in wanted:
    for label, mp in (("SE", se), ("EX", ex), ("CD", cd)):
        if t in mp:
            print(f"{label} {t} => {mp[t][0]} / {mp[t][1]}")

# fuzzy
print("\n=== FUZZY ===")
for t in ["提灯","花果子","毘沙门","寻龙","灯泡","守矢","赛钱","蓬莱","空白","戏水","新闻","星光","固有","一次性","禁止","纯化","锁定","滑溜","使魔","永夜"]:
    hits = []
    for label, mp in (("SE", se), ("EX", ex), ("CD", cd)):
        for zn,(i,en) in mp.items():
            if t in zn:
                hits.append(f"{label}:{zn}=>{en}")
    if hits:
        print(t, ";", "; ".join(hits[:8]))
