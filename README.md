# SpinMuse Project

## Introduction
To build the SpinMuse project, first, use the NuGet Manager to install `Magick.NET-Q8-AnyCPU`. It is used to generate GIF animation files.

## How to Use SpinMuse
1. Drag and drop a PNG file with a watermark. A black and white image with a red rotation axis will be displayed on the left side.
2. Click on the black and white image to modify the red rotation axis. Specify it in your preferred position.
3. Press the button above to start creating the animated GIF. Once the creation is complete, the animation will be displayed on the right side.
4. The animated GIF file is saved with "_ani" added to the PNG filename, making it a GIF file.

## Additional Information
To adjust the rotation speed and the number of animation frames, modify the code in `CreateGifWithRedLineAxi()` in `MainForm.cs`. The rotation speed is specified as the third argument in `SaveGifWithMagickNET()` of the `AnimatedGifCreator` class. It's set in units of 1/100 seconds.
