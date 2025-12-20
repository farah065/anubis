using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Defines a custom Volume Override component that controls the intensity of the URP Post-processing effect on a Scriptable Renderer Feature.
// For more information about the VolumeComponent API, refer to https://docs.unity3d.com/Packages/com.unity.render-pipelines.core@17.2/api/UnityEngine.Rendering.VolumeComponent.html

// Add the Volume Override to the list of available Volume Override components in the Volume Profile.
[VolumeComponentMenu("Post-processing Custom/BitDepth")]

// If the related Scriptable Renderer Feature doesn't exist, display a warning about adding it to the renderer.
[VolumeRequiresRendererFeatures(typeof(BitDepthRendererFeature))]

// Make the Volume Override active in the Universal Render Pipeline.
[SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]

// Create the Volume Override by inheriting from VolumeComponent
public sealed class BitDepthVolumeComponent : VolumeComponent, IPostProcessComponent
{
    // Set the name of the volume component in the list in the Volume Profile.
    public BitDepthVolumeComponent()
    {
        displayName = "BitDepth";
    }

    // Create a property to control the intesity of the effect, with a tooltip description.
    // You can set the default value in the project-wide Graphics settings window. For more information, refer to https://docs.unity3d.com/Manual/urp/urp-global-settings.html
    // You can override the value in a local or global volume. For more information, refer to https://docs.unity3d.com/Manual/urp/volumes-landing-page.html
    // To access the value in a script, refer to the VolumeManager API: https://docs.unity3d.com/Packages/com.unity.render-pipelines.core@latest/index.html?subfolder=/api/UnityEngine.Rendering.VolumeManager.html 
    [Tooltip("Color bit depth per channel")]
    public ClampedFloatParameter depth = new ClampedFloatParameter(8f, 1f, 8f);

    // Optional: Implement the IsActive() method of the IPostProcessComponent interface, and get the intensity value.
    public bool IsActive()
    {
        return depth.GetValue<float>() < 8.0f;
    }
}
