#4K fly

4K fly generates a few thousand different views of a scene, organized and played back on a hex grid intended to play behind a hexagonal lens array, creating a 3D (NO GLASSES!) display.

See my [IntegralPhotography](/IntegralPhotography/) page for details.

The meat of the thing is in NewBehavior.cs, in the Update(). There is a loop there that does camera move, render, copy to slice of texture array. 

I believe modifying gpu-instancing to render each instance to a different slice of the texture array will be the solution for speed up. Somewhere in the scriptable render pipeline, which I have not learned enough about.

Recording writes png files to a disk. 
To convert them to mp4 and then mkv:

ffmpeg -f image2 -pattern_type glob -i '*.png' -c:v libx264 -crf 0 -strict experimental out.mp4

ffmpeg -i out.mp4 -c:v libx265 -crf 20 out.mkv

My media player would stutter strangely on h.265 made straight from the png, hence the mp4 step.

cio
