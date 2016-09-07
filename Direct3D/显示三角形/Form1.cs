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

namespace 显示三角形
{
    public partial class Form1 : Form
    {
        private Device device = null;
        bool pause = false;
        VertexBuffer vertexBuffer = null;
        float Angle = 0, ViewZ = -6.0f;
        public Form1()
        {
            InitializeComponent();
        }

        public bool InitializeGraphics()
        {
            try
            {
                PresentParameters presentParams = new PresentParameters();
                presentParams.Windowed = true;				//不是全屏显示，在一个窗口显示
                presentParams.SwapEffect = SwapEffect.Discard;		 //后备缓存交换的方式
                presentParams.EnableAutoDepthStencil = true;			 //允许使用自动深度模板测试
                //深度缓冲区单元为16位二进制数
                presentParams.AutoDepthStencilFormat = DepthFormat.D16;
                device = new Device(0, DeviceType.Hardware, this, 	 //建立设备类对象
          CreateFlags.SoftwareVertexProcessing, presentParams);
                //设置设备重置事件(device.DeviceReset)事件函数为this.OnResetDevice
                device.DeviceReset += new System.EventHandler(this.OnResetDevice);
                this.OnCreateDevice(device, null);//自定义方法，初始化Device的工作放到这个方法中
                this.OnResetDevice(device, null);//调用设备重置事件(device.DeviceReset)事件函数
            }		//设备重置事件函数要设置Device参数，初始函数中必须调用该函数
            catch (DirectXException)
            {
                return false;
            }
            return true;
        }

        public void OnCreateDevice(object sender, EventArgs e)
        {
            Device dev = (Device)sender;
            vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionColored), 3,
            dev, 0, CustomVertex.TransformedColored.Format, Pool.Default);
            vertexBuffer.Created += new System.EventHandler(this.OnCreateVertexBuffer);
            this.OnCreateVertexBuffer(vertexBuffer, null);

        }

        public void OnResetDevice(object sender, EventArgs e)
        {
            Device dev = (Device)sender;
            dev.RenderState.CullMode = Cull.None;		//取消背面剔除
            dev.RenderState.Lighting = false;			//取消灯光
            SetupMatrices();		//在程序运行期间，Device的3个变换不改变，因此放在此处

        }

        public void Render()		//渲染方法，本方法没有任何渲染代码，可认为是渲染方法的框架
        {
            if (device == null) //如果未建立设备对象，退出
                return;
            if (pause)
                return;
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Blue, 1.0f, 0);
            device.BeginScene();	//开始渲染		
            SetupMatrices();		//在程序运行期间，Device的2个变换参数要改变，因此放在此处
            device.SetStreamSource(0, vertexBuffer, 0);
            device.VertexFormat = CustomVertex.PositionColored.Format;
            device.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
            device.EndScene();		//渲染结束
            device.Present();		//更新显示区域，把后备缓存的图形送到图形卡的显存中显示


        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            this.Render();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            pause = ((this.WindowState == FormWindowState.Minimized) || !this.Visible);
        }

        public void OnCreateVertexBuffer(object sender, EventArgs e)
        {
            CustomVertex.PositionColored[] verts =			   //建模
                        (CustomVertex.PositionColored[])vertexBuffer.Lock(0, 0);
            verts[0].Position = new Vector3(-1.0f, -1.0f, 0.0f);   //顶点0位置，注意为Vector3
            verts[0].Color = System.Drawing.Color.Aqua.ToArgb();   //顶点0颜色
            verts[1].Position = new Vector3(1.0f, -1.0f, 0.0f);	   //顶点1位置
            verts[1].Color = System.Drawing.Color.Brown.ToArgb();
            verts[2].Position = new Vector3(0.0f, 1.0f, 0.0f);	   //顶点2位置
            verts[2].Color = System.Drawing.Color.LightPink.ToArgb();
            vertexBuffer.Unlock();
        }

        private void SetupMatrices()		//修改Device的3个变换
        {
            int iTime = Environment.TickCount % 1000;
            Angle = iTime * (2.0f * (float)Math.PI) / 1000.0f;
            device.Transform.World = Matrix.RotationY(Angle);
            device.Transform.View = Matrix.LookAtLH(new Vector3(0.0f, 3.0f, ViewZ),
                            new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f));
            device.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4,
                            1.0f, 1.0f, 100.0f);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)				//e.KeyCode是键盘每个键的编号
            {
                case Keys.Left:			//Keys.Left是左箭头键编号，三角形沿Y轴左转
                    Angle += 0.1F;
                    break;
                case Keys.Right:			//三角形沿Y轴右转
                    Angle -= 0.1F;
                    break;
                case Keys.Down:			//三角形离观察者越来越远
                    ViewZ += 0.1F;
                    break;
                case Keys.Up:				//三角形离观察者越来越近
                    ViewZ -= 0.1F;
                    break;
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeGraphics();
            Show();
            Render();
        }
    }
}
