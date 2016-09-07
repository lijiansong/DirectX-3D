using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace 地形
{
    public partial class Form1 : Form
    {
        private VertexBuffer vertexBuffer;//定义顶点缓冲变量
        private IndexBuffer indexBuffer;//定义索引缓冲变量
        private int[] indices;//定义索引号变量
        Device device = null;//定义绘图设备
        private CustomVertex.PositionColored[] vertices;// 定义顶点变量
        private int xCount = 5, yCount = 4;//定义横向和纵向网格数目
        private float cellHeight = 0.5f, cellWidth = 1.0f;//定义单元的宽度和长度

        public Form1()
        {
            InitializeComponent();
        }
        private void VertexDeclaration()
        {
            string bitmapPath = @"D:\\Microsoft DirectX SDK (June 2010)\\Samples\\Media\\misc\\seafloor.bmp";
            Bitmap bitmap = new Bitmap(bitmapPath);
            xCount = (bitmap.Width - 1) / 2;
            yCount = (bitmap.Height - 1) / 2;
            cellWidth = bitmap.Width / xCount;
            cellHeight = bitmap.Height / yCount;
            vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionColored),
            (xCount + 1) * (yCount + 1), device,
            Usage.Dynamic | Usage.WriteOnly,
            CustomVertex.PositionColored.Format, Pool.Default);
            vertices = new
            CustomVertex.PositionColored[(xCount + 1) * (yCount + 1)];//定义顶点
            for (int i = 0; i < yCount + 1; i++)
            {
                for (int j = 0; j < xCount + 1; j++)
                {
                    Color color = bitmap.GetPixel((int)(j * cellWidth), (int)(i *
                    cellHeight));
                    float height = float.Parse(color.R.ToString()) +
                    float.Parse(color.G.ToString()) + float.Parse(color.B.ToString());
                    height /= 10;
                    vertices[j + i * (xCount + 1)].Position = new Vector3(j *
                    cellWidth, height, i * cellHeight);
                    vertices[j + i * (xCount + 1)].Color = Color.White.ToArgb();
                }
            }
            vertexBuffer.SetData(vertices, 0, LockFlags.None);
        }
        private void IndicesDeclaration()//定义索引
        {
            indexBuffer = new IndexBuffer(
                typeof(int), 
                6 * xCount * yCount, 
                device,
                Usage.WriteOnly,
                Pool.Default);
            indices = new int[6 * xCount * yCount];
            for (int i = 0; i < yCount; i++)
            {
                for (int j = 0; j < xCount; j++)
                {
                    indices[6 * (j + i * xCount)] = j + i * (xCount + 1);
                    indices[6 * (j + i * xCount) + 1] = j + (i + 1) * (xCount + 1);
                    indices[6 * (j + i * xCount) + 2] = j + i * (xCount + 1) + 1;
                    indices[6 * (j + i * xCount) + 3] = j + i * (xCount + 1) + 1;
                    indices[6 * (j + i * xCount) + 4] = j + (i + 1) * (xCount + 1);
                    indices[6 * (j + i * xCount) + 5] = j + (i + 1) * (xCount + 1)
                    + 1;
                }
            }
            indexBuffer.SetData(indices, 0, LockFlags.None);
        }
        public bool InitializeDirect3D()
        {
            try
            {
                PresentParameters presentParams = new PresentParameters();
                presentParams.Windowed = true; //指定以Windows窗体形式显示
                presentParams.SwapEffect = SwapEffect.Discard; //当前屏幕绘制后它将自动从内存中删除
                device = new Device(0, DeviceType.Hardware, this,
                CreateFlags.SoftwareVertexProcessing, presentParams); //实例化device对象
                VertexDeclaration();//定义顶点
                IndicesDeclaration();//定义索引缓冲
            }
            catch (DirectXException e)
            {
                MessageBox.Show(e.ToString(), "Error"); //处理异常
                return false;
            }
            return true;
        }
        public void Render()
        {
            if (device == null)
            {
                return;
            }
            device.Clear(ClearFlags.Target, Color.DarkSlateBlue, 1.0f, 0); //清除windows界面为深蓝色
            device.BeginScene();
            //在此添加渲染图形代码
            device.RenderState.CullMode = Cull.None;
            device.RenderState.Lighting = false;
            device.VertexFormat = CustomVertex.PositionColored.Format;
            device.SetStreamSource(0, vertexBuffer, 0);
            device.Indices = indexBuffer;
            device.DrawIndexedPrimitives(
                PrimitiveType.TriangleList, 
                0, 
                0,
                (xCount + 1) * (yCount + 1), 
                0, 
                indices.Length / 3);
            device.EndScene();
            device.Present();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeDirect3D();
            this.Show();
            Render();
        }
    }
}
