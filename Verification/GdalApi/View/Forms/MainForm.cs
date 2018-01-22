using GdalApi.Model.Domain.Mapper;
using GdalApi.Model.Domain.Object;
using System;
using System.Windows.Forms;

namespace GdalApi.View.Forms {
    public partial class MainForm : Form {
        #region --- [ Fields ] ---
        #endregion
        #region --- [ Constructor ] ---
        public MainForm() {
            InitializeComponent();
        }
        #endregion
        #region --- [ Events ] ---
        private void MainForm_Load(object sender, EventArgs e) {
        }

        private void btnSaveBitmap_Click(object sender, EventArgs e) {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = "LANDSAT8_sample.bmp";
            sfd.Filter = "Bitmap file(*.bmp;|*.bmp;|All file(*.*)|*.*";
            sfd.Title = "Save a file with a new name.";
            sfd.RestoreDirectory = true;
            sfd.OverwritePrompt = true;
            sfd.CheckPathExists = true;
            if (sfd.ShowDialog() == DialogResult.OK) {
                GeoTiffMapper.getObj().Image.Save(sfd.FileName);
            }
        }

        private void btnReadGeoTiff_Click(object sender, EventArgs e) {
            for (int i = 1; i < Program.FILE_LANDSAT_OUT_IMG.Length; i++) {
                if (GeoTiffMapper.getObj().getGeoTiffObject(i) == null) {
                    GeoTiffMapper.getObj().addGeoTiffObject(
                        new GeoTiffObject(Program.FILE_LANDSAT_OUT_IMG[i - 1], i));
                }
            }
            if (GeoTiffMapper.getObj().Image == null) {
                GeoTiffMapper.getObj().gdalMerge();
            }
            btnSaveBitmap.Enabled = true;
            pictureBox1.Image = GeoTiffMapper.getObj().Image;
            textBox1.Text = GeoTiffMapper.getObj().GdalInfoText;
        }
        #endregion
    }
}
