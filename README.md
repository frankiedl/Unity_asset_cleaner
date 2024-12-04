# Unity Asset Cleaner

A Unity Editor tool to safely find and remove unused assets from your project. It features a user-friendly interface with checkboxes for selection, automatic backup creation, and smart asset detection.

![Unity Asset Cleaner Interface](insert_screenshot_here)

## Features

- **Smart Asset Detection**:
  - Analyzes all scenes in build settings
  - Analyzes currently open scene
  - Resources folder analysis
  - Scene objects and dependencies
  - Double verification for materials in use

- **User-Friendly Interface**:
  - Checkbox selection for each asset
  - Quick deselection with red "X" button
  - Select/Deselect All button
  - Asset count display
  - Assets preview
  - Big red delete button for marked assets

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
  - Double verification for materials in use
  - Confirmation dialogs before deletion

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
   - Configure backup options
   - Make sure "Include Current Scene" is checked if you want to analyze the open scene

2. **Find Unused Assets**:
   - Click the "Find Unused Assets" button
   - The tool will analyze your project and list potentially unused assets

3. **Select Assets for Deletion**:
   - Use checkboxes to mark/unmark assets for deletion
   - Use the red "X" button to quickly unmark assets
   - Use "Select All/Deselect All" for batch selection
   - Review the count of marked assets at the bottom

4. **Delete Assets**:
   - Click the "Delete Marked Assets" button
   - Confirm the deletion in the popup dialog
   - If backup is enabled, assets will be backed up before deletion

### Backup System

- Creates timestamped folders for each deletion operation
- Maintains original folder structure
- For materials and prefabs, also backs up dependencies
- Backups are stored in the specified backup folder
- Each backup includes full asset path structure

## Best Practices

1. **Before Using**:
   - Make a full project backup
   - Ensure all relevant scenes are included in build settings
   - Close unnecessary scenes

2. **During Use**:
   - Keep "Include Current Scene" option enabled
   - Enable backup system for safe deletion
   - Review each asset before marking for deletion
   - Use the preview to verify assets

3. **After Cleaning**:
   - Test your project thoroughly
   - Verify that all scenes still work correctly
   - Keep backups until you're sure everything works

## Limitations

- Dynamic asset loading (using `Resources.Load` or Addressables) might not be detected
- Assets referenced only through reflection or string paths might be marked as unused
- Scene-specific assets might require the relevant scene to be included in build settings

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

- 1.1.0
  - Added checkbox interface
  - Added Select All/Deselect All feature
  - Improved material detection
  - Added backup system
- 1.0.0
  - Initial release
