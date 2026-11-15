#!/usr/bin/env bash
# Foglight Isles 实测脚本 A：无图形批处理验证
set -u
EXE="E:/Codex/游戏制作/雾灯群岛/Builds/ReleaseCandidate/FoglightIslesDemo.exe"
OUT="E:/Codex/游戏制作/雾灯群岛/Builds/QA"
mkdir -p "$OUT"
LOG="$OUT/batch-results.txt"
: > "$LOG"

run() {
  local name="$1"; shift
  local logfile="$OUT/log-$name.txt"
  echo "===== $name =====" | tee -a "$LOG"
  local start=$(date +%s)
  timeout 180 "$EXE" "$@" -batchmode -nographics -logFile "$logfile" >/dev/null 2>&1
  local code=$?
  local dur=$(( $(date +%s) - start ))
  echo "exit=$code  duration=${dur}s" | tee -a "$LOG"
  if [ -f "$logfile" ]; then
    grep -E "\[FoglightIsles (Validation|Save|Quest|Discovery|Performance)\]|PASS|FAIL|Exception|Error|error CS" "$logfile" \
      | grep -vE "^\s*$" | tail -25 | tee -a "$LOG"
  else
    echo "(no log produced)" | tee -a "$LOG"
  fi
  echo "" | tee -a "$LOG"
}

run selftest        -verify-runner-selftest
run journey         -verify-journey
run quest-loop      -verify-quest-loop
run quest-resume    -verify-quest-resume
run discoveries     -verify-discoveries
run discoveries-resume -verify-discoveries-resume
run walk-route      -verify-walk-route
run new-save        -verify-new-save
run title-new-save  -verify-title-new-save
run display-mode    -verify-display-mode
run display-resume  -verify-display-resume

echo "ALL BATCH DONE" | tee -a "$LOG"
