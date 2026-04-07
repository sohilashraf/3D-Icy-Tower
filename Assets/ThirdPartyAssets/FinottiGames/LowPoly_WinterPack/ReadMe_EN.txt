Version: 1.0 Publisher: FinottiGames

Thank you for purchasing WinterPack! This package is designed to be lightweight, modular, and performant, ideal for Mobile, VR, and PC projects. All assets share a single material to ensure maximum performance (Optimized Batching).

📦 Package Content
All assets are available in two variants: Standard and Snowy.

3x Wooden Cabins (Exterior only, interiors are not accessible).

3x Low Poly Pines (Different shapes and sizes).

1x Street Lamp (Emissive light version included).

Environment Bonus: The Demo scene includes environment assets (Terrain, Glaciers, Snow Particle System) that you can freely use to enrich your backgrounds.

🎨 Materials & Textures
The package uses a highly optimized workflow with a single Atlas Material shared across all objects. Included Textures (Recommended resolution 256px or higher):

Albedo (BaseColor): Color palette.

Attributes Map (Mask Map):

R Channel (Red): Metallic

A Channel (Alpha): Smoothness

Emission Map: Handles window and street lamp lights.

Note: If you own other FinottiGames packages, this material is compatible and interchangeable.

⚙️ Installation Guide (Render Pipelines)
The project comes with materials set to Built-in Render Pipeline by default. If you are using URP or HDRP, follow these steps to update the materials.

1. Initial Setup
Ensure you have installed the desired pipeline package (URP or HDRP) via the Package Manager (Window > Package Manager > Unity Registry).

2. Project Configuration (If starting from scratch)
In the Project panel, create your Render Pipeline Asset (e.g., Create > Rendering > URP Asset).

Go to Edit > Project Settings > Graphics.

Assign the newly created file in the Scriptable Render Pipeline Settings field.

Note: If you use HDRP/URP and see everything pink, it is normal. Proceed to step 3.

3. Material Replacement
To ensure correct visualization, we have included specific materials for each pipeline.

Go to the folder: FinottiGames/LowPoly_WinterPack/Materials-Shaders/

Open the folder corresponding to your pipeline (e.g., URP or HDRP).

Select the Prefabs or objects in the scene and drag the correct material into the Mesh Renderer slot.

Alternative: You can simply copy the material settings from the specific folder and paste them onto the main material if you prefer to keep existing links.

💡 Visual Tips
Bloom: To make windows and street lamps "glow" (Emission Texture), you must enable the Bloom effect in your scene's Global Volume.

Snow: The Particle System included in the demo is designed to be attached to the Main Camera to simulate an optimized infinite snowfall.

📞 Support
For any issues, bugs, or requests, do not hesitate to contact me: info@finottigames.com