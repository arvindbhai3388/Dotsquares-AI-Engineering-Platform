# Add a Media Picker with Image Cropping

**Category:** Umbraco CMS
**Use when:** A template needs editor-controlled, art-directed images.

## Prompt

I need to add a Media Picker property that supports image cropping (Umbraco's Image Cropper / focal point functionality) and have the front end render responsive, art-directed images from it. First check the existing Data Type setup: is there already a Media Picker Data Type configured with an Image Cropper crop-value converter, and are named crops (e.g., "hero", "thumbnail", "card") already defined in an existing Data Type's configuration that this should reuse rather than duplicate?

Propose the plan:
1. The Data Type configuration: Media Picker (v3, if applicable) with "enable image cropper" on, and the specific named crop definitions with their aspect ratios/dimensions needed for this use case -- reuse existing named crops where the aspect ratio already matches instead of adding near-duplicate crop names.
2. How the property value is retrieved and rendered in the view: `GetCropUrl()` extension method with the named crop, `<picture>`/`srcset` markup for responsive breakpoints if multiple crop sizes are needed, and whether this project uses Umbraco's built-in `ImageSharp`-based image processing or a CDN-based image processing service (check `appsettings`-driven image processing config indirectly through the service registrations, without opening the restricted config file directly).
3. Focal point handling so cropped images stay correctly centered on the subject regardless of aspect ratio, and `alt` text sourcing (a dedicated "Alt Text" property on the Media Item's Media Type, falling back to the media item's name only if no alt text is set).
4. **Edge cases**: no media picked (render a placeholder or omit the image container entirely -- confirm which is expected), a picked media item that has since been deleted from the Media library, and a non-image media type (PDF/video) accidentally picked if the Media Picker isn't restricted by file type in its Data Type configuration.

Wait for approval before implementing. On implementation, restrict the Media Picker's allowed types to images only (unless intentionally broader), wire up the crop-based rendering with lazy-loading attributes matching the site's existing image markup conventions, and add null/deleted-media guards. Validate rendering with: no image selected, an image with all defined crops set, an image using only the default/uncropped focal point, and confirm no broken image requests or layout shift in the rendered page.
