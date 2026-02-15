
# APK Easy Tool - Hebrew & Modern Fix Edition 🇮🇱



This is a **maintained and fixed fork** of the original [APK Easy Tool](https://github.com/evildog1/APK-Easy-Tool) (last updated in 2022).



## 🛑 The Problem (Why this fork exists?)

The original tool had critical bugs that made it unusable in 2026:

1.  **Hebrew/Non-English Path Crash:** Using Apktool on folders with Hebrew characters (e.g., C:\הורדות\app.apk) caused rut.androlib.exceptions.AndrolibException and failed builds.

2.  **Gibberish Logs:** Console output displayed question marks (????) instead of readable text due to encoding mismatch.

3.  **Outdated Engine:** The internal pktool.jar (v2.6.1) could not handle modern apps (Android 14/15).



## ✅ The Solution (What we changed)

This version introduces a **"Smart Build System"**:

* **Auto-Path Fix:** The tool detects Hebrew/Special characters and automatically creates a temporary, safe English working directory for pktool operations, then restores the original names upon completion.

* **UTF-8 Enforcement:** Forced chcp 65001 and UTF-8 encoding across all CMD processes to fix log readability.

* **Upgraded Core:** Included Apktool 2.10.0 to support the latest Android APKs.

* **Cleaned UI:** Updated version numbers and changelogs to reflect the new maintenance.



## 📦 Download

Go to the [Releases Page](../../releases) to download the ready-to-use EXE.



## 📜 Credits

* Original Creator: **evildog1**

* Hebrew Fixes & Maintenance: **[wspdyb - זונדל גרנד מתמחים טופ]**



## 📜 License

Project follows the original open-source license.

