Thanks for downloading my project!

This is the very first version of a class which can read geometry information and returns the mesh with unified winding order of the triangles' indices.

The method fix the indices making the correspoding triangle to point outward (i.e. any character, forniture, architecture, ecc.)

It doesn't work well with open and inward surfaces such as Terrains or skydomens. To fix this problem I added a method that flip all the normals.


At the moment the algorithm look a little slow for large meshes, so feel free to add branches and try to make it more efficient.

