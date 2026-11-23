# Compares two fingerprint outputs (allfp.py or deep.py), file by file, and prints only what differs.
# Usage: python3 -I compare.py <production output> <our output>
import sys
def load(path):
    out, cur = {}, None
    for line in open(path):
        line = line.rstrip('\n')
        if not line.strip(): continue
        if line.startswith('=='):
            cur = line.split()[1]; out.setdefault(cur, {}); continue
        t = line.split()
        n = 2 if t[0] in ('top', 'detail', 'results', 'field', 'names') else 1
        if len(t) == 4 and t[2] in ('producerName', 'tradingName'): n = 3
        out.setdefault(cur, {})[' '.join(t[:n])] = ' '.join(t[n:])
    return out
a, b = load(sys.argv[1]), load(sys.argv[2])
for f in list(a) + [f for f in b if f not in a]:
    x, y = a.get(f, {}), b.get(f, {})
    diffs = [(k, x.get(k), y.get(k)) for k in list(x) + [k for k in y if k not in x] if x.get(k) != y.get(k)]
    print(f'== {f}: {len(diffs)} different' + ('' if f in a and f in b else ' (only in one)'))
    for k, p, o in diffs: print(f'   {k}: production {p or "-"}, ours {o or "-"}')
