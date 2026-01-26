content = open("08-01-SUMMARY.md").read()
open("08-VERIFICATION-test.md", "w").write("Test: " + content[:100])
print("Test successful")
