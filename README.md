# Small repro of potential bug in .NET 9

When running test on Windows 11 (my local machine) test passes

When running test on `ubuntu-latest` on Github the test fails.

Tried setting up matrix with `windows-latest` and `ubuntu-latest`, but the former is highly unstable
when it comes to installing specific .NET SDK and, in my experience, often cancels.