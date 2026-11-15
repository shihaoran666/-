#!/usr/bin/env bash
# Foglight Isles 实测脚本 B：存档破坏 + 边界 + 多实例
set -u
EXE="E:/Codex/游戏制作/雾灯群岛/Builds/ReleaseCandidate/FoglightIslesDemo.exe"
OUT="E:/Codex/游戏制作/雾灯群岛/Builds/QA"
mkdir -p "$OUT"
LOG="$OUT/crash-results.txt"
: > "$LOG"

say() { echo "$@" | tee -a "$LOG"; }

UPREFS="HKCU\\Software\\Foglight Isles\\Foglight Isles Demo"
LOWDIR="C:/Users/Administrator/AppData/LocalLow/Foglight Isles/Foglight Isles Demo"

say "===== 1. 存档位置探查 ====="
reg query "$UPREFS" 2>&1 | head -20 | tee -a "$LOG"
say "--- LocalLow ---"
ls -la "$LOWDIR" 2>&1 | head -20 | tee -a "$LOG"

say ""
say "===== 2. 反复启动 5 次（每次跑完立刻杀掉）====="
for i in 1 2 3 4 5; do
  timeout 25 "$EXE" -screen-width 800 -screen-height 600 -logFile "$OUT/rep-$i.log" >/dev/null 2>&1 &
  pid=$!
  sleep 8
  kill -9 $pid 2>/dev/null
  wait $pid 2>/dev/null
  say "第 $i 次：已强杀"
done

say ""
say "===== 3. 强杀后能否正常续档 ====="
timeout 90 "$EXE" -batchmode -nographics -verify-quest-resume -logFile "$OUT/after-kill-resume.log" >/dev/null 2>&1
say "exit=$?"
grep -E "PASS|FAIL|Exception|\[FoglightIsles Save\]" "$OUT/after-kill-resume.log" 2>/dev/null | tail -10 | tee -a "$LOG"

say ""
say "===== 4. 存档破坏：备份 → 垃圾数据覆盖 → 启动 ====="
BACKUP="$OUT/uprefs-backup.txt"
reg export "$UPREFS" "$BACKUP" /y 2>&1 | head -2 | tee -a "$LOG"
say "备份文件："; ls -la "$BACKUP" 2>&1 | tee -a "$LOG"

# 写入垃圾值到已知的存档键
for key in "quest.prism.stage.v1" "boat.survey-sail.v1" "tide-beacons.v1" "quest.survey-chart.v1" "quest.storm-signal.v1"; do
  reg add "$UPREFS" /v "$key" /t REG_SZ /d "GARBAGE_NOT_A_NUMBER" /f >/dev/null 2>&1
done
say "已注入垃圾数据"

timeout 90 "$EXE" -batchmode -nographics -verify-runner-selftest -logFile "$OUT/corrupt-run.log" >/dev/null 2>&1
say "启动 exit=$?"
grep -E "PASS|FAIL|Exception|Error|\[FoglightIsles Save\]" "$OUT/corrupt-run.log" 2>/dev/null | tail -15 | tee -a "$LOG"
say "--- 是否出现崩溃堆栈 ---"
grep -cE "NullReferenceException|FormatException|IndexOutOfRange|StackOverflow" "$OUT/corrupt-run.log" 2>/dev/null | tee -a "$LOG"

say ""
say "===== 5. 恢复备份 ====="
reg import "$BACKUP" 2>&1 | head -2 | tee -a "$LOG"
say "已恢复"

say ""
say "===== 6. 双实例并发 ====="
timeout 30 "$EXE" -screen-width 800 -screen-height 600 -logFile "$OUT/inst-a.log" >/dev/null 2>&1 &
A=$!
sleep 5
timeout 30 "$EXE" -screen-width 800 -screen-height 600 -logFile "$OUT/inst-b.log" >/dev/null 2>&1 &
B=$!
sleep 12
kill -9 $A $B 2>/dev/null
wait 2>/dev/null
say "双实例已结束"
say "--- A 异常 ---"; grep -cE "Exception|Error" "$OUT/inst-a.log" 2>/dev/null | tee -a "$LOG"
say "--- B 异常 ---"; grep -cE "Exception|Error" "$OUT/inst-b.log" 2>/dev/null | tee -a "$LOG"

say ""
say "===== 7. 分辨率边界 ====="
for res in "640 360" "2560 1440"; do
  set -- $res
  W=$1; H=$2
  timeout 25 "$EXE" -screen-width $W -screen-height $H -logFile "$OUT/res-$W.log" >/dev/null 2>&1 &
  P=$!
  sleep 10
  kill -9 $P 2>/dev/null
  wait $P 2>/dev/null
  CRASH=$(grep -cE "Crash!!!|Fatal|Exception" "$OUT/res-$W.log" 2>/dev/null || echo 0)
  say "${W}x${H}: 崩溃关键字=$CRASH"
done

say ""
say "ALL CRASH TESTS DONE"
