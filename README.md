# Unity Asset Cleaner

A Unity Editor tool to help you find and safely remove unused assets from your project. It includes features like scene analysis, backup creation, and selective cleaning based on asset types.

## Features

- **Smart Asset Detection**: Analyzes assets across multiple sources:
  - All scenes in build settings
  - Currently open scene
  - Resources folder
  - Scene objects and their dependencies

- **Supported Asset Types**:
  - Scripts (.cs)
  - Textures (.png, .jpg, .jpeg, .tga)
  - Materials (.mat)
  - Audio files (.mp3, .wav, .ogg)
  - Prefabs (.prefab)

- **Safety Features**:
  - Automated backup system with timestamps
  - "Keep" button for assets you want to preserve
  - Double verification for materials in use
  - Dependency tracking for complex assets
  - Backup of asset dependencies for materials and prefabs

## Installation

1. Open your Unity project
2. Create a folder named `Editor` in your Assets folder if it doesn't exist
3. Copy the `AssetCleaner.cs` script into the `Editor` folder
4. Unity will automatically compile the script and add the tool to your editor

## How to Use

### Opening the Tool
1. In Unity, go to the top menu
2. Click on `Tools > Asset Cleaner`

### Configuration Options

#### Analysis Options:
- **Include Current Scene**: Also analyze the scene currently open in the editor
- **Check Resources Folder**: Consider assets in the Resources folder as used

#### Asset Types to Check:
- **Scripts**: C# script files
- **Textures**: Image files (PNG, JPG, JPEG, TGA)
- **Materials**: Material assets
- **Audio**: Sound files (MP3, WAV, OGG)
- **Prefabs**: Prefab assets

#### Backup Options:
- **Create Backup Before Deleting**: Enable/disable automatic backup
- **Backup Folder**: Specify the folder path for backups (default: "Assets/_DeletedAssetsBackup")

### Using the Tool

1. **Configure Settings**:
   - Select which types of assets to analyze
   - Configure backup options if desired
   - Make sure "Include Current Scene" is checked if you want to analyze the open scene

2. **Find Unused Assets**:
   - Click the "Find Unused Assets" button
   - The tool will analyze your project and list potentially unused assets

3. **Review Results**:
   - Each asset will be displayed with preview
   - Use the "Keep" button for assets you want to retain
   - Use the "Delete" button to remove assets

4. **Backup System**:
   - When deleting with backup enabled, assets are copied to a timestamped folder
   - The backup maintains the original folder structure
   - For materials and prefabs, dependencies are also backed up
   - Backups are organized by date and time

## Best Practices

1. **Before Using**:
   - Make a full project backup
   - Ensure all relevant scenes are included in build settings
   - Close unnecessary scenes

2. **During Use**:
   - Keep the "Include Current Scene" option enabled
   - Enable backup system for safe deletion
   - Review each asset before deletion
   - Use the "Keep" button if unsure about an asset

3. **After Cleaning**:
   - Test your project thoroughly
   - Backup folders can be safely removed after verifying everything works

## Limitations

- Dynamic asset loading (using `Resources.Load` or Addressables) might not be detected
- Assets referenced only through reflection or string paths might be marked as unused
- Scene-specific assets might require the relevant scene to be included in build settings

## Troubleshooting

If you accidentally delete needed assets:
1. Go to the backup folder (default: "Assets/_DeletedAssetsBackup")
2. Find the timestamp folder from when the deletion occurred
3. Copy the needed assets back to their original location

## Contributing

Feel free to contribute to this project by:
- Reporting issues
- Suggesting enhancements
- Submitting pull requests

## License

This tool is released under the MIT License. Feel free to use it in your projects.
