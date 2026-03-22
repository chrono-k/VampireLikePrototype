{
  echo "===== git status ====="
  git status --short
  echo

  echo "===== tracked script changes (git diff) ====="
  git diff -- Assets/Scripts
  echo

  echo "===== untracked C# files ====="
  for f in $(git ls-files --others --exclude-standard | grep '\.cs$'); do
    echo "===== $f ====="
    sed -n '1,260p' "$f"
    echo
  done
} | pbcopy