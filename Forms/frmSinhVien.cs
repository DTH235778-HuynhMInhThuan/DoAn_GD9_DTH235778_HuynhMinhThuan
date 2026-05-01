using QuanLyNhaTro.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OfficeOpenXml;
using System.IO;


namespace QuanLyNhaTro.Forms
{
    public partial class frmSinhVien : Form
    {
        private NhaTroContext context = new NhaTroContext();
        private bool isAdding = false;
        public frmSinhVien()
        {
            InitializeComponent();
            dgvSinhVien.AutoGenerateColumns = false;
        }


        private void SinhVien_Load(object sender, EventArgs e)
        {
            ThietKeUI.ApDungToanBo(this);
            LoadData();
            SetControlState(false);
        }
        private void LoadData()
        {
            try
            {
                // 1. Lấy danh sách từ DB
                var list = context.SinhViens.ToList();

                // 2. Quan trọng: Nếu ní dùng Rows.Add thì phải tắt DataSource cũ đi
                dgvSinhVien.DataSource = null;
                dgvSinhVien.Rows.Clear(); // Xóa sạch các dòng cũ để nạp mới

                // 3. Duyệt từng sinh viên để nạp vào bảng kèm theo ảnh
                foreach (var sv in list)
                {
                    // Tìm đường dẫn ảnh trong thư mục Images ní đã tạo tay
                    string tenFile = sv.HinhAnh;
                    Image img = null;

                    if (!string.IsNullOrEmpty(tenFile))
                    {
                        string path = Path.Combine(Application.StartupPath, "Images", tenFile);
                        if (File.Exists(path))
                        {
                            // Dùng MemoryStream để nạp ảnh giúp không bị lỗi "khóa file"
                            byte[] buffer = File.ReadAllBytes(path);
                            using (MemoryStream ms = new MemoryStream(buffer))
                            {
                                img = Image.FromStream(ms);
                            }
                        }
                    }

                    // 4. Thêm dòng vào DataGridView. 
                    // Thứ tự các biến phải khớp y chang thứ tự cột ní đặt trong Edit Columns
                    dgvSinhVien.Rows.Add(
                        sv.MaSV,    // Cột 1
                        sv.TenSV,   // Cột 2
                        sv.SDT,     // Cột 3
                        sv.CCCD,    // Cột 4
                        sv.QueQuan, // Cột 5
                        img         // Cột 6 - Cột Hình Ảnh (kiểu ImageColumn)
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu: " + ex.Message);
            }
        }
        private void SetControlState(bool editing)
        {
            // Mở/Khóa các ô nhập liệu (Mã SV luôn khóa vì tự tăng)
            txtMaSV.Enabled = false;
            txtTenSV.Enabled = editing;
            txtSDT.Enabled = editing;
            txtCCCD.Enabled = editing;
            txtQueQuan.Enabled = editing;
            btnChonAnh.Enabled = editing; // Khóa luôn nút chọn ảnh khi không sửa

            // Điều khiển các nút chức năng
            btnThem.Enabled = !editing;
            btnXoa.Enabled = !editing && dgvSinhVien.CurrentRow != null;
            btnLuu.Enabled = editing;
            btnHuy.Enabled = editing;
            btnThoat.Enabled = !editing;

            // ĐIỂM CHỐT: Khi đang sửa (editing = true) thì khóa bảng Grid lại 
            // để tránh việc đang sửa người này lại click sang người kia gây lỗi dữ liệu
            dgvSinhVien.Enabled = !editing;
        }
        private void ClearInput()
        {
            txtMaSV.Clear();
            txtTenSV.Clear();
            txtSDT.Clear();
            txtCCCD.Clear();
            txtQueQuan.Clear();
            txtTenSV.Focus(); // Đưa con trỏ chuột vào ô Tên
        }
        private void dgvSinhVien_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Bỏ !isAdding để lúc nào nhấp vào bảng dữ liệu cũng hiện lên TextBox
            if (e.RowIndex >= 0)
            {
                try
                {
                    DataGridViewRow row = dgvSinhVien.Rows[e.RowIndex];

                    // 1. Đổ dữ liệu chữ lên các TextBox bằng số thứ tự cột (Index)
                    // Ní kiểm tra kỹ thứ tự cột trên lưới của ní nhé:
                    txtMaSV.Text = row.Cells[0].Value?.ToString();   // Mã SV
                    txtTenSV.Text = row.Cells[1].Value?.ToString();  // Tên SV
                    txtSDT.Text = row.Cells[2].Value?.ToString();    // SĐT
                    txtCCCD.Text = row.Cells[3].Value?.ToString();   // CCCD
                    txtQueQuan.Text = row.Cells[4].Value?.ToString(); // Quê Quán


                    // 2. Hiển thị ảnh lên PictureBox
                    picAnhSV.Image = null;

                    // Lấy ảnh trực tiếp từ ô số 5 (cột Hình Ảnh)
                    if (row.Cells[5].Value is Image imgInGrid)
                    {
                        picAnhSV.Image = (Image)imgInGrid.Clone();
                    }


                    // 3. Đưa App về trạng thái xem (không phải đang thêm mới)
                    isAdding = false;
                    SetControlState(true);
                    btnXoa.Enabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi hiển thị: " + ex.Message);
                }
            }
        }
        private void btnThem_Click(object sender, EventArgs e)
        {
            isAdding = true;
            ClearInput();
            SetControlState(true);
        }
        private void btnHuy_Click(object sender, EventArgs e)
        {
            isAdding = false;
            ClearInput();
            SetControlState(false);

           
        }
        private void btnThoat_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void btnLuu_Click(object sender, EventArgs e)
        {
            // Kiểm tra ràng buộc dữ liệu
            if (string.IsNullOrWhiteSpace(txtTenSV.Text))
            {
                MessageBox.Show("Vui lòng nhập Tên sinh viên!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTenSV.Focus();
                return;
            }

            // 1. Kiểm tra trống CCCD
            if (string.IsNullOrWhiteSpace(txtCCCD.Text))
            {
                MessageBox.Show("Vui lòng nhập Căn Cước Công Dân!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCCCD.Focus();
                return;
            }

            // 2. THÊM MỚI: Ràng buộc phải đúng 12 chữ số
            string cccd = txtCCCD.Text.Trim();
            if (cccd.Length != 12)
            {
                MessageBox.Show("Số Căn Cước Công Dân phải có đúng 12 chữ số!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCCCD.Focus();
                return;
            }

            try
            {
                // KHAI BÁO BIẾN SV Ở ĐÂY ĐỂ DÙNG CHUNG CHO TOÀN BỘ HÀM
                SinhVien sv = null;

                if (isAdding)
                {
                    // THÊM MỚI
                    sv = new SinhVien(); // Khởi tạo mới
                    sv.TenSV = txtTenSV.Text.Trim();
                    sv.SDT = txtSDT.Text.Trim();
                    sv.CCCD = txtCCCD.Text.Trim();
                    sv.QueQuan = txtQueQuan.Text.Trim();

                    context.SinhViens.Add(sv);
                }
                else
                {
                    // CẬP NHẬT (SỬA)
                    int id = int.Parse(txtMaSV.Text);
                    sv = context.SinhViens.FirstOrDefault(s => s.MaSV == id);

                    if (sv != null)
                    {
                        sv.TenSV = txtTenSV.Text.Trim();
                        sv.SDT = txtSDT.Text.Trim();
                        sv.CCCD = txtCCCD.Text.Trim();
                        sv.QueQuan = txtQueQuan.Text.Trim();
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy sinh viên để cập nhật!", "Lỗi");
                        return;
                    }
                }

                // XỬ LÝ HÌNH ẢNH (Bây giờ sv đã có giá trị, không còn báo lỗi đỏ nữa)

                // 1. Tạo thư mục 'Images' nếu chưa có
                string folderPath = Path.Combine(Application.StartupPath, "Images");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                // 2. Chỉ xử lý nếu người dùng có chọn ảnh mới (picAnhSV.Tag không null)
                if (picAnhSV.Tag != null) // picAnhSV là cái PictureBox ní kéo từ Toolbox vào
                {
                    try
                    {
                        string duongDanGoc = picAnhSV.Tag.ToString();
                        // Tạo tên file mới để lưu vào DB (Dùng mã SV cho dễ quản lý)
                        string tenFileAnh = "SV_" + txtMaSV.Text + Path.GetExtension(duongDanGoc);
                        string duongDanDich = Path.Combine(folderPath, tenFileAnh);

                        // Copy ảnh vào thư mục Images ní vừa tạo tay
                        File.Copy(duongDanGoc, duongDanDich, true);

                        // Gán tên file vào đối tượng sv để lưu xuống Database
                        sv.HinhAnh = tenFileAnh;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi lưu ảnh: " + ex.Message);
                    }
                }
                // Lưu tất cả thay đổi xuống Database
                context.SaveChanges();
                MessageBox.Show("Lưu dữ liệu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Trả về trạng thái bình thường
                isAdding = false;
                SetControlState(true);
                LoadData();
            }
            catch (Exception ex)
            {
                string errorDetail = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show("Lỗi chi tiết từ Database: " + errorDetail, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtMaSV.Text)) return;

            DialogResult dialog = MessageBox.Show("Bạn có chắc chắn muốn xóa sinh viên này không?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialog == DialogResult.Yes)
            {
                try
                {
                    int id = int.Parse(txtMaSV.Text);
                    SinhVien sv = context.SinhViens.FirstOrDefault(s => s.MaSV == id);
                    if (sv != null)
                    {
                        context.SinhViens.Remove(sv);
                        context.SaveChanges();

                        MessageBox.Show("Xóa thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearInput();
                        LoadData();
                        SetControlState(false);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể xóa. Sinh viên này có thể đang có Hợp đồng! Lỗi chi tiết: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ChiNhapSo_TextChanged(object sender, EventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt == null) return;

            // Lọc ra chỉ lấy đúng các con số từ cái chuỗi đang nhập
            string chiLaySo = string.Join("", txt.Text.Where(char.IsDigit));

            // Nếu phát hiện có chữ lọt vào (chuỗi gốc khác chuỗi đã lọc)
            if (txt.Text != chiLaySo)
            {
                txt.Text = chiLaySo; // Ghi đè lại bằng chuỗi sạch (chỉ toàn số)
                txt.SelectionStart = txt.Text.Length; // Kéo con trỏ chuột về cuối cùng để gõ tiếp
            }
        }

        private void btnXuatExcel_Click(object sender, EventArgs e)
        {
            // 1. Cấu hình bản quyền EPPlus (Bắt buộc)
            OfficeOpenXml.ExcelPackage.License.SetNonCommercialPersonal("Do An Ca Nhan");

            try
            {
                using (ExcelPackage pck = new ExcelPackage())
                {
                    // 2. Tạo một Sheet mới đặt tên là "Danh sách Sinh viên"
                    ExcelWorksheet ws = pck.Workbook.Worksheets.Add("SinhVien");

                    // 3. Xuất Tiêu đề cột từ DataGridView
                    // (Chỉ lấy các cột: Mã SV, Tên SV, SĐT, CCCD, Quê Quán)
                    for (int i = 0; i < dgvSinhVien.Columns.Count; i++)
                    {
                        ws.Cells[1, i + 1].Value = dgvSinhVien.Columns[i].HeaderText;
                        ws.Cells[1, i + 1].Style.Font.Bold = true; // In đậm tiêu đề
                        ws.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
                    }

                    // 4. Đổ dữ liệu từ bảng (dgvSinhVien) vào file Excel
                    for (int row = 0; row < dgvSinhVien.Rows.Count; row++)
                    {
                        for (int col = 0; col < dgvSinhVien.Columns.Count; col++)
                        {
                            ws.Cells[row + 2, col + 1].Value = dgvSinhVien.Rows[row].Cells[col].Value?.ToString();
                        }
                    }

                    // 5. Tự động căn chỉnh độ rộng cột cho đẹp
                    ws.Cells.AutoFitColumns();

                    // 6. Mở hộp thoại lưu file
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.Filter = "Excel Files|*.xlsx";
                    saveFileDialog.FileName = "Danh_Sach_Sinh_Vien_" + DateTime.Now.ToString("yyyyMMdd");

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        File.WriteAllBytes(saveFileDialog.FileName, pck.GetAsByteArray());
                        MessageBox.Show("Đã xuất danh sách sinh viên ra Excel thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xuất Excel: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnChonAnh_Click(object sender, EventArgs e)
        {
            // Tạo cái hộp thoại mở file
            OpenFileDialog open = new OpenFileDialog();
            // Chỉ cho phép chọn file đuôi ảnh
            open.Filter = "Image Files(*.jpg; *.jpeg; *.png)|*.jpg; *.jpeg; *.png";

            if (open.ShowDialog() == DialogResult.OK)
            {
                // Hiện ảnh lên khung đã kéo ở Bước 1
                picAnhSV.Image = Image.FromFile(open.FileName);
                // Lưu tạm đường dẫn gốc vào Tag để lát copy
                picAnhSV.Tag = open.FileName;
            }
        }

    }
}
