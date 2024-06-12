# A Command Line utility for managing GPS tags in photos using ExifTool

This is a tool that can be used to match GPS logs to photo time stamps and geo tag them using exisf tool.

So far I have been using it with .arw (Sony raw format) and csv logs captured using [i-GotU GT600 logger](https://www.filesaveas.com/igotu_gt600.html)


For details, [the following post](https://dev.to/syamaner/building-a-command-line-photo-tagger-using-docker-net-and-exiftool-1gc4) covers the motivation being the tool.


## Next Steps

- Currently only csv format exported from i-GotU device is supported.
  - I had [GPX](https://wiki.openstreetmap.org/wiki/GPX) support as well but removed so next steps will involve:
    - Make csv parsing configurable so mappings can be changed at runtime
    - Bring back GPX Support so that this could be a wider supported format.
- I have only tested with Sony .arw raw files and will add tests for other raw / compressed formats
- GPS logs are noisy, so depending on the use case (moving vehicle. walking around, mostly stationary, we might benefit from different algorithms to eliminate the outliers and extrapolate the positions so this will be a topic.
- Docker multiarchitecture support
- Ability to restore the originals backed up by Exiftool
