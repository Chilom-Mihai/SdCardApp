using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Management;

namespace test
{
    public partial class Form1 : Form
    {

        private ManagementEventWatcher watcher;
        private List<Image> images = new List<Image>();
        private string destinationFolder = @"C:\Photos";
        private string sourceFolderPath;


        public Form1()
        {
            InitializeComponent();

            // Add an image column to the DataGridView
            DataGridViewImageColumn imageColumn = new DataGridViewImageColumn();
            imageColumn.HeaderText = "Image";
            imageColumn.ImageLayout = DataGridViewImageCellLayout.Stretch; // Adjust the layout as needed
            dataGridView1.Columns.Add(imageColumn);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Automatically find the SD card or new device
            string sourceFolder = FindSdCardOrNewDevice();

            if (sourceFolder != null)
            {
                try
                {
                    // Read all image files from the selected folder
                    string[] imageFiles = Directory.GetFiles(sourceFolder, "*.*", SearchOption.AllDirectories)
                        .Where(s => s.ToLower().EndsWith(".jpg") || s.ToLower().EndsWith(".jpeg") || s.ToLower().EndsWith(".png") || s.ToLower().EndsWith(".gif"))
                        .ToArray();

                    // Copy images to the destination folder
                    CopyImagesToDestination(imageFiles);

                    // Load images
                    LoadImages(imageFiles);

                    // Update DataGridView
                    UpdateDataGridView();

                    // Store the selected source folder path
                    sourceFolderPath = sourceFolder;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading/copying images: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("No SD card or new device found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void CopyImagesToDestination(string[] sourceFiles)
        {
            // Create the destination folder if it doesn't exist
            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }

            // Copy images to the destination folder
            foreach (string imagePath in sourceFiles)
            {
                string destinationPath = Path.Combine(destinationFolder, Path.GetFileName(imagePath));
                File.Copy(imagePath, destinationPath, true);
            }
        }

        private void LoadImages(string[] imageFiles)
        {
            // Load images
            images.Clear();
            foreach (string imagePath in imageFiles)
            {
                Image image = Image.FromFile(imagePath);
                images.Add(image);
            }
        }

        private void UpdateDataGridView()
        {
            // Clear existing rows
            dataGridView1.Rows.Clear();

            // Add images to DataGridView
            foreach (Image image in images)
            {
                // Set the column width and row height
                dataGridView1.Columns[0].Width = 200;
                dataGridView1.RowTemplate.Height = 200;
                // Resize the image to fit the cell
                Image resizedImage = new Bitmap(image, new Size(200, 200));

                // Add a new row with the resized image
                dataGridView1.Rows.Add(resizedImage);
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Handle any additional click events as needed
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Specify the destination folder
            if (!Directory.Exists(destinationFolder))
            {
                MessageBox.Show("Destination folder does not exist. Please select another folder as the destination.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
                {
                    if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                    {
                        destinationFolder = folderBrowserDialog.SelectedPath;
                    }
                    else
                    {
                        return; // User canceled the folder selection
                    }
                }
            }

            try
            {
                // Read all image files from the selected folder
                string[] imageFiles = Directory.GetFiles(destinationFolder, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(s => s.ToLower().EndsWith(".jpg") || s.ToLower().EndsWith(".jpeg") || s.ToLower().EndsWith(".png") || s.ToLower().EndsWith(".gif"))
                    .ToArray();

                LoadImages(imageFiles);

                UpdateDataGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading images: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void button3_Click(object sender, EventArgs e)
        {
            // Specify the destination folder
            if (!Directory.Exists(destinationFolder))
            {
                MessageBox.Show("Destination folder does not exist. Please select another folder as the destination.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
                {
                    if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                    {
                        destinationFolder = folderBrowserDialog.SelectedPath;
                    }
                    else
                    {
                        return; // User canceled the folder selection
                    }
                }
            }

            try
            {
                // Read all image files from the selected folder
                string[] imageFiles = Directory.GetFiles(destinationFolder, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(s => s.ToLower().EndsWith(".jpg") || s.ToLower().EndsWith(".jpeg") || s.ToLower().EndsWith(".png") || s.ToLower().EndsWith(".gif"))
                    .ToArray();

                // Check if the number of imported images is the same as the number of images in the destination folder
                if (images.Count != imageFiles.Length)
                {
                    MessageBox.Show("Error: The number of imported images does not match the number of images in the destination folder.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                LoadImages(imageFiles);

                UpdateDataGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading images: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string CalculateFolderChecksum(string folderPath)
        {
            List<string> imageFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
                .Where(s => s.ToLower().EndsWith(".jpg") || s.ToLower().EndsWith(".jpeg") || s.ToLower().EndsWith(".png") || s.ToLower().EndsWith(".gif"))
                .ToList();

            // Use a hashing algorithm (e.g., MD5) to calculate the checksum
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                foreach (string imagePath in imageFiles)
                {
                    byte[] fileBytes = File.ReadAllBytes(imagePath);
                    md5.TransformBlock(fileBytes, 0, fileBytes.Length, null, 0);
                }

                md5.TransformFinalBlock(new byte[0], 0, 0);

                byte[] checksumBytes = md5.Hash;
                return BitConverter.ToString(checksumBytes).Replace("-", "").ToLower();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // Check if the source folder has been selected previously
            if (string.IsNullOrEmpty(sourceFolderPath))
            {
                MessageBox.Show("Please select a source folder first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Check if the source folder exists
            if (Directory.Exists(sourceFolderPath))
            {
                // Calculate the checksum for the source folder
                string sourceChecksum = CalculateFolderChecksum(sourceFolderPath);

                // Calculate the checksum for the destination folder
                string destinationChecksum = CalculateFolderChecksum(destinationFolder);

                // Compare the checksums
                if (sourceChecksum == destinationChecksum)
                {
                    MessageBox.Show("Checksums match. Source and destination folders have the same content.", "Checksum", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Checksums do not match. Source and destination folders have different content.", "Checksum", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("Source folder does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private string FindSdCardOrNewDevice()
        {
            // Get all available drives
            DriveInfo[] drives = DriveInfo.GetDrives();

            // Iterate through the drives to find the SD card or new device
            foreach (DriveInfo drive in drives)
            {
                if (drive.DriveType == DriveType.Removable)
                {
                    // Check if the drive is not the system drive (C:)
                    if (!drive.RootDirectory.FullName.Equals(Path.GetPathRoot(Environment.SystemDirectory), StringComparison.OrdinalIgnoreCase))
                    {
                        return drive.RootDirectory.FullName;
                    }
                }
            }

            return null; // No SD card or new device found
        }


    }
}