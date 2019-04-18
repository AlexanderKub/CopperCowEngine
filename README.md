# Copper Cow Engine
C# 3D Engine that uses DirectX 11 and SharpDX.
Best engine for copper cows visualization 2019. (In my opinion).

![screenshot1](https://github.com/AlexanderKub/CopperCowEngine/raw/master/Branding/Screenshots/ScreenshotPBR.png)

This WIP version ([last stable version](855c8236615f03e8fb036c2fc57a4b127f5073ab)):

Features:
1) PBR with metallic workflow, prefiltered IBL and precalculated BRDF. (UE4 paper)[https://cdn2.unrealengine.com/Resources/files/2013SiggraphPresentationsNotes-26915738.pdf]
2) HDR and tonemapping with adaptation. Bloom in progress.
3) Deffered rendering with MRT and light accumulation accumulation pass. (some lights issues, WIP)
4) Forward rendering with Z-prepass, Velocity map pass and tiled light culling. (broken after refactoring, WIP)
5) Shadow maps (issues with different render paths).
6) Assets manager WIP - all raw assets packs into custom binary assets files for improve engine loading performance.
7) Asset manager can import .hdr spherical maps and convert to engine CubeMaps, with calculation IBL additional map. Also asset manager can calculate BRDF map.
8) UI Editor really early WIP, only project creation and assets import with preview.
9) Flexible ECS pattern with "singleton" components. (Overwatch ecs architecture)[https://www.youtube.com/watch?v=W3aieHjyNvw]

Roadmap: 
1) Finish render paths.
2) [Clustered forward](http://www.humus.name/Articles/PracticalClusteredShading.pdf) rendering and combine it with small G-Buffer [like DOOM does](http://www.adriancourreges.com/blog/2016/09/09/doom-2016-graphics-study/).
3) Shadow volumes.
4) Assets manager finish importer/editor for all types of asset.
5) UI editor for creating scenes.
6) Skinning and animations.
7) SSAO and SSR
8) And so on and so forth.
