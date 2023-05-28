using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Drawing.Drawing2D;
using System.Reflection.Emit;
using System.IO;

namespace Cell_designer
{
    public partial class Form1 : Form
    {

        private int clickCount = 0;
        private Point startPoint;
        bool isRectangleMarked = true;
        System.Drawing.Rectangle rectangle;
        JArray marks = new JArray();
        JArray marksForsave = new JArray();
        private PointF[] PolygonPoints = new PointF[0];
        public Form1()
        {
            InitializeComponent();
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (isRectangleMarked)
            {
                //Console.WriteLine(clickCount);
                // 增加计数器
                clickCount++;
                // 如果是第一次单击
                if (clickCount == 1)
                {
                    // 记录起点
                    startPoint = e.Location;
                }
                // 如果是第二次单击
                else if (clickCount == 2)
                {
                    // 计算矩形的位置和大小
                    int x = Math.Min(startPoint.X, e.X);
                    int y = Math.Min(startPoint.Y, e.Y);
                    int width = Math.Abs(startPoint.X - e.X);
                    int height = Math.Abs(startPoint.Y - e.Y);
                    // 更新 rectangle 变量
                    rectangle = new System.Drawing.Rectangle(x, y, width, height);
                    // 触发 PictureBox 的重绘事件
                    pictureBox1.Invalidate();
                    // 重置计数器
                    clickCount = 0;
                }
            }
            else
            {

                Point point = new Point(e.X, e.Y);

                // 将鼠标单击的位置添加到 PolygonPoints 数组中
                Array.Resize(ref PolygonPoints, PolygonPoints.Length + 1);
                PolygonPoints[PolygonPoints.Length - 1] = point;
                // 如果 PolygonPoints 数组中的点的数量大于等于 3 个，表示多边形已经被完整标记
                pictureBox1.Invalidate();

            }
        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            // 设置 picturebox2 的位置为鼠标的当前位置
            pictureBox2.Enabled = true;

            Point pt = new Point();
            pt.X = Cursor.Position.X - this.Location.X;
            pt.Y = Cursor.Position.Y - this.Location.Y;
            pictureBox2.Location = pt;
            // 设置 picturebox2 的可见性为 true
            pictureBox2.Visible = true;
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            pictureBox2.Visible = false;
            pictureBox2.Enabled = false;
            label1.Text = "X";
            label2.Text = "Y";
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            int originalHeight = this.pictureBox1.Image.Height;

            PropertyInfo rectangleProperty = this.pictureBox1.GetType().GetProperty("ImageRectangle", BindingFlags.Instance | BindingFlags.NonPublic);
            Rectangle picrectangle = (Rectangle)rectangleProperty.GetValue(this.pictureBox1, null);

            int currentWidth = picrectangle.Width;
            int currentHeight = picrectangle.Height;

            double rate = (double)currentHeight / (double)originalHeight;

            int black_left_width = (currentWidth == this.pictureBox1.Width) ? 0 : (this.pictureBox1.Width - currentWidth) / 2;
            int black_top_height = (currentHeight == this.pictureBox1.Height) ? 0 : (this.pictureBox1.Height - currentHeight) / 2;
            int zoom_x = e.X - black_left_width;
            int zoom_y = e.Y - black_top_height;

            int original_x = (int)(zoom_x / rate);
            int original_y = (int)(zoom_y / rate);
            label1.Text = original_x.ToString();
            label2.Text = original_y.ToString();
            int width_g = 100; // 放大的宽度
            int height_g = 100; // 放大的高度

            Bitmap bmp = new Bitmap(width_g, height_g);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(pictureBox1.Image, new Rectangle(0, 0, width_g * 2, height_g * 2), new Rectangle(original_x - 25, original_y - 25, width_g, height_g), GraphicsUnit.Pixel);
                g.FillEllipse(Brushes.Yellow, width_g / 2 - 2, height_g / 2 - 2, 4, 4);
            }
            pictureBox2.Image = bmp;
            Point pt = new Point();
            if (Cursor.Position.X + width_g * 2 > this.Width) // 当鼠标位置右侧空间不足时
            {
                // 在鼠标左侧显示
                pt.X = Cursor.Position.X - this.Location.X - width_g * 2 - 10;
            }
            else
            {
                // 在鼠标右侧显示
                pt.X = Cursor.Position.X - this.Location.X + 10;
            }

            if (Cursor.Position.Y + height_g * 2 > this.Height) // 当鼠标位置下方空间不足时
            {
                // 在鼠标上方显示
                pt.Y = Cursor.Position.Y - this.Location.Y - height_g * 2 - 20;
            }
            else
            {
                // 在鼠标下方显示
                pt.Y = Cursor.Position.Y - this.Location.Y + 20;
            }

            pictureBox2.Location = pt;


            if (clickCount == 1)
            {
                // 计算矩形的位置和大小
                int x = Math.Min(startPoint.X, e.X);
                int y = Math.Min(startPoint.Y, e.Y);
                int width = Math.Abs(startPoint.X - e.X);
                int height = Math.Abs(startPoint.Y - e.Y);
                // 更新 rectangle 变量
                rectangle = new Rectangle(x, y, width, height);
                // 触发 PictureBox 的重绘事件
                pictureBox1.Invalidate();
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            // 获取 PictureBox 的画布
            Graphics g = e.Graphics;

            // 遍历 marks 数组
            foreach (JArray mark in marks)
            {
                // 获取标记形状
                string shape = mark[0].ToString();
                // 如果是矩形
                if (shape == "rectangle")
                {
                    // 获取矩形的位置和大小
                    int x = mark[1].ToObject<int>();
                    int y = mark[2].ToObject<int>();
                    int width = mark[3].ToObject<int>();
                    int height = mark[4].ToObject<int>();
                    // 画出矩形
                    g.DrawRectangle(Pens.Red, new Rectangle(x, y, width, height));
                }
                // 如果是多边形
                else if (shape == "polygon")
                {
                    // 获取多边形的顶点坐标
                    float[] listp = new float[mark.Count() - 1];

                    for (int i = 1; i < mark.Count(); i++)
                    {
                        listp[i - 1] = float.Parse(mark[i].ToString());
                    }

                    PointF[] points = new PointF[listp.Length / 2];
                    for (int i = 0; i < listp.Length; i += 2)
                    {
                        points[i / 2] = new PointF((float)listp[i], (float)listp[i + 1]);
                    }
                    // 画出多边形
                    g.DrawPolygon(Pens.Red, points);
                }
            }


            g.DrawRectangle(Pens.Red, rectangle);
            if (PolygonPoints != null)
            {
                if (PolygonPoints.Length < 3)
                {
                    // 遍历 points 数组
                    foreach (PointF point in PolygonPoints)
                    {
                        // 在 PictureBox 中显示点的位置
                        g.FillEllipse(Brushes.Red, point.X - 2, point.Y - 2, 4, 4);
                    }
                }
                // 如果 points 数组中的点的数量大于等于 3
                else
                {
                    // 画出多边形
                    g.DrawPolygon(Pens.Red, PolygonPoints);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string fileName = openFileDialog.FileName;
                    pictureBox1.Load(fileName);
                    pictureBox1.Enabled = true;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            clickCount = 0;
            rectangle = new Rectangle();
            PolygonPoints = new PointF[0];
            marks = new JArray();
            marksForsave = new JArray();
            pictureBox1.Invalidate();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            clickCount = 0;
            rectangle = new Rectangle();
            PolygonPoints = new PointF[0];
            pictureBox1.Invalidate();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (this.pictureBox1.Image == null)
            {
                return;
            }
            int originalHeight = this.pictureBox1.Image.Height;
            int originalWidth = this.pictureBox1.Image.Width;

            PropertyInfo rectangleProperty = this.pictureBox1.GetType().GetProperty("ImageRectangle", BindingFlags.Instance | BindingFlags.NonPublic);
            Rectangle picturerectangle = (Rectangle)rectangleProperty.GetValue(this.pictureBox1, null);

            int currentWidth = picturerectangle.Width;
            int currentHeight = picturerectangle.Height;

            float rate = (float)currentHeight / (float)originalHeight;

            int black_left_width = (currentWidth == this.pictureBox1.Width) ? 0 : (this.pictureBox1.Width - currentWidth) / 2;
            int black_top_height = (currentHeight == this.pictureBox1.Height) ? 0 : (this.pictureBox1.Height - currentHeight) / 2;


            if (isRectangleMarked)
            {
                int zoom_x = rectangle.X - black_left_width;
                int zoom_y = rectangle.Y - black_top_height;

                float original_x = (float)zoom_x / rate;
                float original_y = (float)zoom_y / rate;
                if (original_x < 0)
                {
                    original_x = 0;

                }
                if (original_y < 0)
                {
                    original_y = 0;
                }

                if (original_x > this.pictureBox1.Image.Width)
                {
                    original_x = this.pictureBox1.Image.Width;

                }
                if (original_y >= this.pictureBox1.Image.Height)
                {
                    original_y = this.pictureBox1.Image.Height;
                }

                float original_width = rectangle.Width / rate;
                float original_height = rectangle.Height / rate;
                // 将矩形标记信息添加到 JArray 中
                marksForsave.Add(new JArray("rectangle", original_x / originalWidth, original_y / originalHeight, original_width / originalWidth, original_height / originalHeight));
                marks.Add(new JArray("rectangle", rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height));
                pictureBox1.Invalidate();
                clickCount = 0;
            }
            // 如果当前是多边形标记状态
            else
            {
                if (PolygonPoints == null || PolygonPoints.Length < 3)
                {
                    return;
                }
                JArray pointArray = new JArray("polygon");
                JArray pointArraySave = new JArray("polygon");
                foreach (PointF point in PolygonPoints)
                {
                    pointArray.Add(point.X);
                    pointArray.Add(point.Y);
                    float zoom_x = point.X - black_left_width;
                    float zoom_y = point.Y - black_top_height;

                    float original_x = (float)zoom_x / rate;
                    float original_y = (float)zoom_y / rate;
                    if (original_x < 0)
                    {
                        original_x = 0;

                    }
                    if (original_y < 0)
                    {
                        original_y = 0;
                    }

                    if (original_x > this.pictureBox1.Image.Width)
                    {
                        original_x = this.pictureBox1.Image.Width;

                    }
                    if (original_y > this.pictureBox1.Image.Height)
                    {
                        original_y = this.pictureBox1.Image.Height;
                    }
                    pointArraySave.Add(original_x / originalWidth);
                    pointArraySave.Add(original_y / originalHeight);
                }
                // 将多边形标记信息添加到 JArray 中
                marks.Add(pointArray);
                marksForsave.Add(pointArraySave);
                pictureBox1.Invalidate();
                PolygonPoints = new PointF[0];
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (isRectangleMarked)
            {
                // 将 isRectangleMarked 标志设置为 true
                isRectangleMarked = false;

                // 更新按钮文本
                button5.Text = "Polygon mode";
            }
            else
            {
                // 将 isRectangleMarked 标志设置为 true
                isRectangleMarked = true;
                // 更新按钮文本
                button5.Text = "Rectangle mode";
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            JObject output = new JObject { { "Labels", new JObject { { "LabelList", new JArray("Class 1", "Class 2") }, { "LabelAbbreviation", new JArray("C1", "C2") }, { "LabelNumber", "2" } } }, { "COIs", new JObject { { "COINumber", marksForsave.Count.ToString() } } } };

            // 遍历JArray数组，将每个数组元素添加到JSON对象中
            for (int i = 0; i < marksForsave.Count; i++)
            {
                output["COIs"][(i + 1).ToString()] = marksForsave[i];
            }

            // 将JSON对象转换为字符串
            string outputStr = output.ToString();
            SaveFileDialog saveDialog = new SaveFileDialog();

            // 设置文件保存位置（这里设置为桌面）
            saveDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            saveDialog.FileName = "new configuration.json";
            saveDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";

            // 设置默认文件保存类型为JSON文件
            saveDialog.DefaultExt = "json";
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                // 使用File类的WriteAllText方法将JSON字符串保存到文件中
                File.WriteAllText(saveDialog.FileName, outputStr);
            }

            Console.WriteLine(outputStr);
        }
    }
}
