using GdalApi.Model.Domain.Object;
using System;
using System.Windows.Forms;

namespace GdalApi.View.Forms {
    public partial class MainForm : Form {
        #region --- [ Fields ] ---
        GeoTiffObject m_tGeoTiffObj;
        #endregion
        #region --- [ Constructor ] ---
        public MainForm() {
            InitializeComponent();
        }
        #endregion
        #region --- [ Events ] ---
        private void MainForm_Load(object sender, EventArgs e) {
            if (m_tGeoTiffObj == null) {
                btnSaveBitmap.Enabled = false;
            }
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
                m_tGeoTiffObj.save(sfd.FileName);
            }
        }

        private void btnReadGeoTiff_Click(object sender, EventArgs e) {
            string strPath = @"test_b1.tif";
            if (m_tGeoTiffObj == null) {
                m_tGeoTiffObj = new GeoTiffObject(strPath);
            } else {
                m_tGeoTiffObj.reload();
            }
            btnSaveBitmap.Enabled = true;
            textBox1.Text = m_tGeoTiffObj.gdalInfoText;
            pictureBox1.Image = m_tGeoTiffObj.Image;
        }
        #endregion
    }
}
