# Cubase Project File Editor

View and edit .CPR files for Cubase versions 2 and 3:

![image](https://github.com/omeriko9/Cubase-Project-File-Reverse-Engineering/assets/5153984/af48a6c5-7cd8-40d4-80db-3d8cc2b2ede7)

Long story:

While prying my old cubase project files, I encountered a graduation cubase project I made when studying home music production.
I could load it in Cubase SX2, but for some reason, several VST's (mainly, Waves Bundle Compressors and EQs) had an addition to their name, e.g. instead of **C1 Comp** it appeared as **C1 Comp (z)**, instead of **REQ 4 bands** it was named **REQ 4 bands (z)** etc.

I wanted Cubase to load them properly, but Cubase does not provide a mechanism to alter the names of the VSTs. 
This means that I loose all of their settings, thus not being able to hear the project as it was originally created at 2004.
Since the problem and solution are obvious (removing the (z) suffix), I searched for a tool that provides such ability of renaming VSTs, but couldn't find one, so I decided to write one.

In order to do so I needed to reverse engineer the Cubase SX2 project file format.
As per writing these lines, it is still work in progress, as I'm doing solely by creating projects, saving them and examining the binary file (.cpr file).

I managed to extract part of the 'VST Mixer' (F3 in Cubase) and to display it. 
I also managed to identify several tracks such as MIDI and Audio tracks, and display their names.

**Unfortunately**, editing the VST effect name is not enough for some reason, and even after changing the name and 'compiling' a valid .cpr file, fixing sections and strings size, Cubase still claims that the VST effect is 'missing'.

The tool also try to display the file hierarchy in a Tree View form.

Written in .NET Framework 4.8, Winform.

Please do not currently ask me for features as this project is in its perliminary stages, and I'm still working on bugs and missing features.
