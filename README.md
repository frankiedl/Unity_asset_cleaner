# Unity Asset Cleaner

A Unity Editor tool to safely find and remove unused assets from your project based on scene dependencies. It features a scene-based cleaning approach, automated backup system, and empty folder cleanup.

(![interface] https://github.com/user-attachments/assets/e287535b-4ad5-47a8-bf26-a7e82f6184cb

## Features

- **Scene-Based Analysis**:
  - Select specific scenes to preserve assets from
  - Automatic current scene detection
  - Build settings scenes integration
  - Project-wide scene discovery

- **Smart Asset Detection**:
  - Analyzes dependencies for selected scenes
  - Safe script detection (won't delete itself)
  - Empty folder cleanup
  - Deep dependency checking for materials and prefabs

- **User-Friendly Interface**:
  - Scene selection panel with Select/Deselect All
  - Checkbox selection for each asset
  - Quick deselection with red "X" button
  - Asset preview
  - Asset count display
  - Clear deletion confirmation

- **Supported Asset Types**:
  - Scripts (.cs)
  - Textures (.png, .jpg, .jpeg, .tga)
  - Materials (.mat)
  - Audio files (.mp3, .wav, .ogg)
  - Prefabs (.prefab)

- **Safety Features**:
  - Automated backup system with timestamps
  - Full directory structure preservation in backups
  - Dependencies backup for materials and prefabs
  - Protection against deleting the cleaner script itself
  - Confirmation dialogs before deletion
  - Empty folder cleanup (optional)

## Installation

1. Open your Unity project
2. Create a folder named `Editor` in your Assets folder if it doesn't exist
3. Copy the `AssetCleaner.cs` script into the `Editor` folder
4. Unity will automatically compile the script and add the tool to your editor

## How to Use

### Opening the Tool
1. In Unity, go to the top menu
2. Click on `Tools > Asset Cleaner`

### Configuration

#### Scene Selection:
- Use the "Scenes to Preserve" foldout to select which scenes to analyze
- The current scene is automatically detected and selected
- All scenes from build settings are listed
- Use "Refresh Scene List" to update the scene list
- Use "Select/Deselect All Scenes" for quick configuration

#### Asset Types to Check:
- **Scripts**: C# script files
- **Textures**: Image files (PNG, JPG, JPEG, TGA)
- **Materials**: Material assets
- **Audio**: Sound files (MP3, WAV, OGG)
- **Prefabs**: Prefab assets

#### Cleaning Options:
- **Create Backup Before Deleting**: Enable/disable automatic backup
- **Delete Empty Folders**: Remove empty folders after asset deletion
- **Backup Folder**: Specify the folder path for backups (default: "Assets/_DeletedAssetsBackup")

### Workflow

1. **Configure Scene Selection**:
   - Expand "Scenes to Preserve"
   - Select the scenes whose assets you want to keep
   - Use the "Select All" or "Deselect All" buttons as needed

2. **Set Up Options**:
   - Choose which types of assets to analyze
   - Configure backup options
   - Decide if empty folders should be cleaned

3. **Find Unused Assets**:
   - Click "Find Unused Assets"
   - The tool will analyze your project based on selected scenes
   - Assets used in selected scenes are protected

4. **Review Assets**:
   - Use checkboxes to mark/unmark assets for deletion
   - Use the red "X" button to quickly unmark assets
   - Preview assets to verify selection
   - Use "Select All/Deselect All" for batch selection

5. **Delete Assets**:
   - Click "Delete Marked Assets"
   - Confirm the deletion
   - If backup is enabled, assets will be backed up first
   - Empty folders will be cleaned if the option is enabled

### Backup System

The backup system creates:
- Timestamped folders for each deletion operation
- Complete folder structure preservation
- Dependency backups for materials and prefabs
- Automatic backup folder creation if it doesn't exist

## Best Practices

1. **Before Using**:
   - Make a full project backup
   - Close unnecessary scenes
   - Review your build settings scenes

2. **During Use**:
   - Carefully select which scenes to preserve
   - Enable backup system for first few uses
   - Review each asset before marking for deletion
   - Pay special attention to shared assets

3. **After Cleaning**:
   - Test your project thoroughly
   - Verify that all selected scenes work correctly
   - Keep backups until everything is verified

## Limitations

- Dynamic asset loading (using `Resources.Load` or Addressables) might not be detected
- Assets referenced only through reflection or string paths might be marked as unused
- Scenes not selected for preservation will have their unique assets marked as unused

## Troubleshooting

If you accidentally delete needed assets:
1. Go to the backup folder (default: "Assets/_DeletedAssetsBackup")
2. Find the timestamp folder from when the deletion occurred
3. Copy the needed assets back to their original location
4. Click Assets > Refresh in Unity

## Contributing

Feel free to contribute to this project by:
- Reporting issues
- Suggesting enhancements
- Submitting pull requests

## License

This tool is released under the MIT License. Feel free to use it in your projects.

## Version History

- 2.0.0
  - Added scene-based analysis
  - Added empty folder cleanup
  - Added self-protection for the cleaner script
  - Improved UI with helpbox styling
  - Added quick scene selection tools
- 1.1.0
  - Added checkbox interface
  - Added Select All/Deselect All feature
  - Improved material detection
  - Added backup system
- 1.0.0
  - Initial release
