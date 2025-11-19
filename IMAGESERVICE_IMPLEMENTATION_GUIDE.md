# ImageService Implementation Guide

This document outlines how the ImageService has been integrated into the GardenHub application and provides guidance for implementing it in additional models (Plant, Equipment).

## Overview

The ImageService provides two main functions:
1. **ConvertFileToByteArrayAsync** - Converts IFormFile to byte[] for database storage
2. **ConvertByteArrayToFile** - Converts byte[] back to base64 data URI for display in views

## Models with Image Support

The following models have image properties:
- ? Garden (Implemented)
- ? JournalEntry (Implemented)
- ? Plant (Pending - needs controller/views)
- ? Equipment (Pending - needs controller/views)

## Implementation Pattern

### 1. Model Requirements

Each model should have these properties:
```csharp
[NotMapped]
public IFormFile? ImageFile { get; set; }
public byte[]? ImageData { get; set; }
public string? ImageType { get; set; }
```

### 2. Controller Setup

Inject IImageService in the constructor:
```csharp
private readonly IImageService _imageService;

public YourController(IYourService yourService, IImageService imageService)
{
    _yourService = yourService;
    _imageService = imageService;
}
```

### 3. Index Action Pattern

Convert images for all items:
```csharp
public async Task<IActionResult> Index()
{
    var items = await _yourService.GetAll();
    
    foreach (var item in items)
    {
        item.ImageFile = null;
        ViewData[$"ItemImage_{item.Id}"] = _imageService.ConvertByteArrayToFile(
            item.ImageData, 
            item.ImageType, 
            DefaultImage.YourDefaultImageType);
    }
    
    return View(items);
}
```

### 4. Details Action Pattern

Convert single image:
```csharp
public async Task<IActionResult> Details(int? id)
{
    if (id == null) return NotFound();
    
    var item = await _yourService.GetById(id.Value);
    if (item == null) return NotFound();

    ViewData["ItemImage"] = _imageService.ConvertByteArrayToFile(
        item.ImageData, 
        item.ImageType, 
        DefaultImage.YourDefaultImageType);

    return View(item);
}
```

### 5. Create Action Pattern

Handle image upload:
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create([Bind("...properties,ImageFile")] YourModel model)
{
    if (model.ImageFile != null)
    {
        model.ImageData = await _imageService.ConvertFileToByteArrayAsynC(model.ImageFile);
        model.ImageType = model.ImageFile.ContentType;
    }
    
    ModelState.Remove("ImageFile");
    
    if (ModelState.IsValid)
    {
        await _yourService.Create(model);
        return RedirectToAction(nameof(Index));
    }
    
    return View(model);
}
```

### 6. Edit GET Action Pattern

Show current image:
```csharp
public async Task<IActionResult> Edit(int? id)
{
    if (id == null) return NotFound();
    
    var item = await _yourService.GetById(id.Value);
    if (item == null) return NotFound();
    
    ViewData["CurrentImage"] = _imageService.ConvertByteArrayToFile(
        item.ImageData, 
        item.ImageType, 
        DefaultImage.YourDefaultImageType);
    
    return View(item);
}
```

### 7. Edit POST Action Pattern

Handle new or keep existing image:
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, [Bind("...properties,ImageFile")] YourModel model)
{
    if (id != model.Id) return NotFound();

    if (model.ImageFile != null)
    {
        model.ImageData = await _imageService.ConvertFileToByteArrayAsynC(model.ImageFile);
        model.ImageType = model.ImageFile.ContentType;
    }
    else
    {
        // Keep existing image
        var existing = await _yourService.GetById(id);
        if (existing != null)
        {
            model.ImageData = existing.ImageData;
            model.ImageType = existing.ImageType;
        }
    }
    
    ModelState.Remove("ImageFile");
    
    if (ModelState.IsValid)
    {
        await _yourService.Update(model, id);
        return RedirectToAction(nameof(Index));
    }
    
    return View(model);
}
```

### 8. View Patterns

#### Create/Edit Forms
Add `enctype="multipart/form-data"` to form tag:
```html
<form asp-action="Create" enctype="multipart/form-data">
    <!-- other fields -->
    
    <div class="form-group">
        <label asp-for="ImageFile" class="control-label">Image</label>
        <input asp-for="ImageFile" type="file" class="form-control" accept="image/*" />
        <span asp-validation-for="ImageFile" class="text-danger"></span>
    </div>
</form>
```

#### Edit Forms with Current Image
```html
<div class="form-group">
    <label class="control-label">Current Image</label>
    @if (ViewData["CurrentImage"] != null && !string.IsNullOrEmpty(ViewData["CurrentImage"]!.ToString()))
    {
        <div class="mb-2">
            <img src="@ViewData["CurrentImage"]" alt="Current Image" 
                 style="max-width: 200px; max-height: 200px;" class="img-thumbnail" />
        </div>
    }
    <label asp-for="ImageFile" class="control-label">Change Image</label>
    <input asp-for="ImageFile" type="file" class="form-control" accept="image/*" />
    <span asp-validation-for="ImageFile" class="text-danger"></span>
    <small class="form-text text-muted">Leave empty to keep current image</small>
</div>
```

#### Index View - Table Cell
```html
<td>
    @if (ViewData[$"ItemImage_{item.Id}"] != null && !string.IsNullOrEmpty(ViewData[$"ItemImage_{item.Id}"]!.ToString()))
    {
        <img src="@ViewData[$"ItemImage_{item.Id}"]" alt="@item.Name" 
             style="max-width: 80px; max-height: 80px;" class="img-thumbnail" />
    }
</td>
```

#### Details View
```html
@if (ViewData["ItemImage"] != null && !string.IsNullOrEmpty(ViewData["ItemImage"]!.ToString()))
{
    <dt class="col-sm-2">Image</dt>
    <dd class="col-sm-10">
        <img src="@ViewData["ItemImage"]" alt="@Model.Name" 
             style="max-width: 300px; max-height: 300px;" class="img-thumbnail" />
    </dd>
}
```

## Default Images

Set appropriate default images in DefaultImage enum:
- ProfileImage = 1
- PlantImage = 2
- EquipmentImage = 3
- GardenImage = 4
- NutrientImage = 5

## Completed Implementations

### Gardens
- ? Controller updated with IImageService
- ? Create view - image upload
- ? Edit view - image upload with preview
- ? Details view - image display
- ? Index view - thumbnail display

### JournalEntries
- ? Controller updated with IImageService
- ? Create view - image upload
- ? Edit view - image upload with preview
- ? Details view - image display
- ? Index view - thumbnail display

## Next Steps for Plant and Equipment

When you're ready to implement Plants and Equipment:

1. Create controllers (or scaffold them)
2. Follow the controller patterns above
3. Create views following the view patterns above
4. Use DefaultImage.PlantImage for Plants
5. Use DefaultImage.EquipmentImage for Equipment

## Notes

- Always add `enctype="multipart/form-data"` to forms that upload files
- Remove "ImageFile" from ModelState validation: `ModelState.Remove("ImageFile")`
- The ImageService handles null/empty byte arrays by showing default images
- Images are stored as byte[] in the database and converted to base64 data URIs for display
