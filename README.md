# kuramoto-sync-xmas-tree
For Matt Parker's LED christmas tree project (https://www.youtube.com/watch?v=WuMRJf6B5Q4) I have created an animation of the tree split into 5 different segments. The state of the LEDs in each segment is dictated by a travelling sine wave. Each segment begins with a sine wave having a unique phase from all other segment waves. As time passes, the phases of the sine waves are adjusted using the Kuramoto model for coupled oscillator synchronization. By the end of the animation, all segments are in phase with each other and appear to be changing as one whole segment. A few still frames were added manually at the end of the animation, to indicate that a seteady state has been reached. The bulk of the animation, however, was created in c# using Rhino7 and Grasshopper. The animation as a whole was really fun to model! I hope you enjoy.
