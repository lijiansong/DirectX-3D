﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace 高程图01
{
    public partial class MainForm : Form
    {
        private Device device = null;//定义绘图设备
        bool pause = false;

        public Vector3 CamPostion = new Vector3(0, 100, 100);//定义摄像机位置
        public Vector3 CamTarget = new Vector3(125, 30, 125);//定义摄像机目标位置

        private float angleY = 0.01f;//定义绕Y轴旋转变量

        private int mouseLastX, mouseLastY;//记录鼠标按下时的坐标位置
        private bool isRotateByMouse = false;//记录是否由鼠标控制旋转
        private bool isMoveByMouse = false;//记录是否由鼠标控制移动

        private CustomVertex.PositionTextured[] vertices;//定义顶点变量
        private Texture texture;//定义贴图变量
        private Material material;//定义材质变量

        private VertexBuffer vertexBuffer;//定义顶点缓冲变量
        private IndexBuffer indexBuffer;//定义索引缓冲变量
        private int[] indices;//定义索引号变量

        private int xCount = 5, yCount = 4;//定义横向和纵向网格数目
        private float cellHeight = 1f, cellWidth = 1f;//定义单元的宽度和长度
        public MainForm()
        {
            InitializeComponent();
        }
        public bool InitializeDirect3D()
        {
            try
            {
                PresentParameters presentParams = new PresentParameters();
                presentParams.Windowed = true; //指定以Windows窗体形式显示
                presentParams.SwapEffect = SwapEffect.Discard; //当前屏幕绘制后它将自动从内存中删除
                presentParams.AutoDepthStencilFormat = DepthFormat.D16;
                presentParams.EnableAutoDepthStencil = true;
                presentParams.PresentationInterval = PresentInterval.Immediate;
                device = new Device(0, DeviceType.Hardware, this, CreateFlags.SoftwareVertexProcessing, presentParams); //实例化device对象
                device.DeviceReset += new System.EventHandler(this.OnResetDevice);
                this.OnCreateDevice(device, null);
                this.OnResetDevice(device, null);
            }
            catch (DirectXException e)
            {
                MessageBox.Show(e.ToString(), "Error"); //处理异常
                return false;
            }
            return true;
        }
        //导入贴图和材质
        public void OnCreateDevice(object sender, EventArgs e)
        {
            Device device = (Device)sender;
            material = new Material();
            material.Diffuse = Color.White;
            material.Specular = Color.LightGray;
            material.SpecularSharpness = 15.0F;
            device.Material = material;
            texture = TextureLoader.FromFile(device, @"F:\\workdir\\VC# Based DirectX\\colorMap.jpg");
        }
        /**
         * 通过Bitmap类获得BMP图片对象，然后根据其Width属性和Height属性设置生成三角网中横向和纵向单元数，
         * 同时设置单元尺寸，在设置高程值中通过获取图片中相应位置的颜色值，
         * 然后通过一个相对比例得到一个相对的高程值，最后将该值赋予定义的顶点坐标值
         */
        public void OnResetDevice(object sender, EventArgs e)
        {
            Device device = (Device)sender;
            string bitmapPath = @"F:\\workdir\\VC# Based DirectX\\heightMap.BMP";
            Bitmap bitmap = new Bitmap(bitmapPath);
            xCount = (bitmap.Width - 1) / 2;
            yCount = (bitmap.Height - 1) / 2;
            cellWidth = bitmap.Width / xCount;
            cellHeight = bitmap.Height / yCount;

            vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionTextured), (xCount + 1) * (yCount + 1), device,
                Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionColored.Format, Pool.Default);
            vertices = new CustomVertex.PositionTextured[(xCount + 1) * (yCount + 1)];//定义顶点
            for (int i = 0; i < yCount + 1; i++)
            {
                for (int j = 0; j < xCount + 1; j++)
                {
                    Color color = bitmap.GetPixel((int)(j * cellWidth), (int)(i * cellHeight));
                    float height = float.Parse(color.R.ToString()) + float.Parse(color.G.ToString()) + float.Parse(color.B.ToString());
                    height /= 10;
                    vertices[j + i * (xCount + 1)].Position = new Vector3(j * cellWidth, height, i * cellHeight);
                    vertices[j + i * (xCount + 1)].Tu = (float)j / (xCount + 1);
                    vertices[j + i * (xCount + 1)].Tv = (float)i / (yCount + 1);
                }
            }
            vertexBuffer.SetData(vertices, 0, LockFlags.None);
            CamTarget = new Vector3(bitmap.Width / 2, 0f, bitmap.Height / 2);//设置摄像机目标位置

            indexBuffer = new IndexBuffer(typeof(int), 6 * xCount * yCount, device, Usage.WriteOnly, Pool.Default);
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
                    indices[6 * (j + i * xCount) + 5] = j + (i + 1) * (xCount + 1) + 1;
                }
            }
            indexBuffer.SetData(indices, 0, LockFlags.None);
        }

        public void Render()//渲染
        {
            if (device == null)   //如果device为空则不渲染
                return;
            if (pause) //如果窗口被切换或者最小化则停止渲染，减少CPU压力
                return;

            Matrix viewMatrix = Matrix.LookAtLH(CamPostion, CamTarget, new Vector3(0, 1, 0));
            device.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4, this.Width / this.Height, 0.3f, 500f);
            device.Transform.View = viewMatrix;

            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.DarkSlateBlue, 1.0f, 0);  //清除windows界面为深蓝色
            device.BeginScene();
            //在此添加渲染图形代码
            device.RenderState.CullMode = Cull.None;
            device.RenderState.FillMode = FillMode.Solid;
            device.RenderState.Lighting = false;
            device.SetTexture(0, texture);//设置贴图

            device.VertexFormat = CustomVertex.PositionTextured.Format;
            device.SetStreamSource(0, vertexBuffer, 0);
            device.Indices = indexBuffer;
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, (xCount + 1) * (yCount + 1), 0, indices.Length / 3);
            device.EndScene();
            device.Present();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeDirect3D();
            this.Show();
            Render();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            Vector4 tempV4;
            Matrix currentView = device.Transform.View;//当前摄像机的视图矩阵
            switch (e.KeyCode)
            {
                case Keys.Left:
                    CamPostion.Subtract(CamTarget);
                    tempV4 = Vector3.Transform(CamPostion, Matrix.RotationQuaternion(
                            Quaternion.RotationAxis(new Vector3(currentView.M12, currentView.M22, currentView.M32), -angleY)));
                    CamPostion.X = tempV4.X + CamTarget.X;
                    CamPostion.Y = tempV4.Y + CamTarget.Y;
                    CamPostion.Z = tempV4.Z + CamTarget.Z;
                    break;
                case Keys.Right:
                    CamPostion.Subtract(CamTarget);
                    tempV4 = Vector3.Transform(CamPostion, Matrix.RotationQuaternion(
                            Quaternion.RotationAxis(new Vector3(currentView.M12, currentView.M22, currentView.M32), angleY)));
                    CamPostion.X = tempV4.X + CamTarget.X;
                    CamPostion.Y = tempV4.Y + CamTarget.Y;
                    CamPostion.Z = tempV4.Z + CamTarget.Z;
                    break;
                case Keys.Up:
                    CamPostion.Subtract(CamTarget);
                    tempV4 = Vector3.Transform(CamPostion, Matrix.RotationQuaternion(
                       Quaternion.RotationAxis(new Vector3(device.Transform.View.M11
                       , device.Transform.View.M21, device.Transform.View.M31), -angleY)));
                    CamPostion.X = tempV4.X + CamTarget.X;
                    CamPostion.Y = tempV4.Y + CamTarget.Y;
                    CamPostion.Z = tempV4.Z + CamTarget.Z;
                    break;
                case Keys.Down:
                    CamPostion.Subtract(CamTarget);
                    tempV4 = Vector3.Transform(CamPostion, Matrix.RotationQuaternion(
                       Quaternion.RotationAxis(new Vector3(device.Transform.View.M11
                       , device.Transform.View.M21, device.Transform.View.M31), angleY)));
                    CamPostion.X = tempV4.X + CamTarget.X;
                    CamPostion.Y = tempV4.Y + CamTarget.Y;
                    CamPostion.Z = tempV4.Z + CamTarget.Z;
                    break;
                case Keys.Add:
                    CamPostion.Subtract(CamTarget);
                    CamPostion.Scale(0.95f);
                    CamPostion.Add(CamTarget);
                    break;
                case Keys.Subtract:
                    CamPostion.Subtract(CamTarget);
                    CamPostion.Scale(1.05f);
                    CamPostion.Add(CamTarget);
                    break;
            }
            Matrix viewMatrix = Matrix.LookAtLH(CamPostion, CamTarget, new Vector3(0, 1, 0));
            device.Transform.View = viewMatrix;
            Render();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            this.Render();
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mouseLastX = e.X;
                mouseLastY = e.Y;
                isRotateByMouse = true;
            }
            else if (e.Button == MouseButtons.Middle)
            {
                mouseLastX = e.X;
                mouseLastY = e.Y;
                isMoveByMouse = true;
            }
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            isRotateByMouse = false;
            isMoveByMouse = false;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (isRotateByMouse)
            {
                Matrix currentView = device.Transform.View;//当前摄像机的视图矩阵
                float tempAngleY = 2 * (float)(e.X - mouseLastX) / this.Width;
                CamPostion.Subtract(CamTarget);

                Vector4 tempV4 = Vector3.Transform(CamPostion, Matrix.RotationQuaternion(
                    Quaternion.RotationAxis(new Vector3(currentView.M12, currentView.M22, currentView.M32), tempAngleY)));
                CamPostion.X = tempV4.X;
                CamPostion.Y = tempV4.Y;
                CamPostion.Z = tempV4.Z;

                float tempAngleX = 4 * (float)(e.Y - mouseLastY) / this.Height;
                tempV4 = Vector3.Transform(CamPostion, Matrix.RotationQuaternion(
                    Quaternion.RotationAxis(new Vector3(currentView.M11, currentView.M21, currentView.M31), tempAngleX)));
                CamPostion.X = tempV4.X + CamTarget.X;
                CamPostion.Y = tempV4.Y + CamTarget.Y;
                CamPostion.Z = tempV4.Z + CamTarget.Z;
                Matrix viewMatrix = Matrix.LookAtLH(CamPostion, CamTarget, new Vector3(0, 1, 0));
                device.Transform.View = viewMatrix;

                mouseLastX = e.X;
                mouseLastY = e.Y;
                Render();
            }
            else if (isMoveByMouse)
            {
                Matrix currentView = device.Transform.View;//当前摄像机的视图矩阵
                float moveFactor = 0.01f;
                CamTarget.X += -moveFactor * ((e.X - mouseLastX) * currentView.M11 - (e.Y - mouseLastY) * currentView.M12);
                CamTarget.Y += -moveFactor * ((e.X - mouseLastX) * currentView.M21 - (e.Y - mouseLastY) * currentView.M22);
                CamTarget.Z += -moveFactor * ((e.X - mouseLastX) * currentView.M31 - (e.Y - mouseLastY) * currentView.M32);

                CamPostion.X += -moveFactor * ((e.X - mouseLastX) * currentView.M11 - (e.Y - mouseLastY) * currentView.M12);
                CamPostion.Y += -moveFactor * ((e.X - mouseLastX) * currentView.M21 - (e.Y - mouseLastY) * currentView.M22);
                CamPostion.Z += -moveFactor * ((e.X - mouseLastX) * currentView.M31 - (e.Y - mouseLastY) * currentView.M32);

                Matrix viewMatrix = Matrix.LookAtLH(CamPostion, CamTarget, new Vector3(0, 1, 0));
                device.Transform.View = viewMatrix;
                mouseLastX = e.X;
                mouseLastY = e.Y;
                Render();
            }
        }
        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            float scaleFactor = -(float)e.Delta / 2000 + 1f;
            CamPostion.Subtract(CamTarget);
            CamPostion.Scale(scaleFactor);
            CamPostion.Add(CamTarget);
            Matrix viewMatrix = Matrix.LookAtLH(CamPostion, CamTarget, new Vector3(0, 1, 0));
            device.Transform.View = viewMatrix;
            Render();
        }

        private void Form1_Resize(object sender, EventArgs e)//窗口切换时停止渲染
        {
            pause = ((this.WindowState == FormWindowState.Minimized) || !this.Visible);
        }
    }
}
