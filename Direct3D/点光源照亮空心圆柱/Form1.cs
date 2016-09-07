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

namespace 点光源照亮空心圆柱
{
    public partial class Form1 : Form
    {
        private Device device = null;
        bool pause = false;
        VertexBuffer vertexBuffer = null;
        Material mtrl;

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
            Device dev = (Device)sender;						//注意阴影部分
            vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionNormal), 100, dev,
            Usage.WriteOnly, CustomVertex.PositionNormal.Format, Pool.Default);
            vertexBuffer.Created += new System.EventHandler(this.OnCreateVertexBuffer);
            this.OnCreateVertexBuffer(vertexBuffer, null);
            mtrl = new Material();
            mtrl.Diffuse = System.Drawing.Color.Red;			//物体的颜色
            mtrl.Ambient = System.Drawing.Color.White;			//反射环境光的颜色 

        }

        public void OnResetDevice(object sender, EventArgs e)
        {
            Device dev = (Device)sender;
            dev.RenderState.CullMode = Cull.None;				//取消背面剔除
            device.RenderState.ZBufferEnable = true;			//打开Z缓冲
            device.RenderState.Lighting = true;				//打开灯光

        }

        public void Render()		//渲染方法，本方法没有任何渲染代码，可认为是渲染方法的框架
        {
            if (device == null) //如果未建立设备对象，退出
                return;
            if (pause)
                return;
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer,
        System.Drawing.Color.Blue, 1.0f, 0);
            device.BeginScene();		//开始渲染
            SetupLights();			//设置灯光，程序运行期间灯光要改变，必须放在此处
            SetupMatrices();
            device.SetStreamSource(0, vertexBuffer, 0);
            device.VertexFormat = CustomVertex.PositionNormal.Format;
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, (4 * 25) - 2);
            device.EndScene();//渲染结束
            device.Present();//更新显示区域，把后备缓存的D图形送到图形卡的显存中显示

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
            CustomVertex.PositionNormal[] verts =
                                 (CustomVertex.PositionNormal[])vertexBuffer.Lock(0, 0);
            for (int i = 0; i < 50; i++)
            {
                float theta = (float)(2 * Math.PI * i) / 49;
                verts[2 * i].Position = new Vector3((float)Math.Sin(theta), -1, (float)Math.Cos(theta));
                verts[2 * i].Normal = new Vector3((float)Math.Sin(theta), 0, (float)Math.Cos(theta));
                verts[2 * i + 1].Position = new Vector3((float)Math.Sin(theta), 1, (float)Math.Cos(theta));
                verts[2 * i + 1].Normal = new Vector3((float)Math.Sin(theta), 0, (float)Math.Cos(theta));
            }
            vertexBuffer.Unlock();
        }		//注意每个点的法线方向是从空心圆柱上(下)底园的圆心到该点连线的方向

        private void SetupMatrices()
        {
            device.Transform.World = Matrix.RotationAxis(new Vector3((float)Math.Cos(Environment.TickCount / 250.0f), 1,
                (float)Math.Sin(Environment.TickCount / 250.0f)), Environment.TickCount / 3000.0f);
            device.Transform.View = Matrix.LookAtLH(new Vector3(0.0f, 3.0f, -5.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f));
            device.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4, 1.0f, 1.0f, 100.0f);

            //device.Transform.World = Matrix.RotationY(0);
            //device.Transform.View = Matrix.LookAtLH(new Vector3(0.0f, 3.0f, -5.0f),
            //            new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f));
            //device.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4,
            //            1.0f, 1.0f, 100.0f);

        }

        private void SetupLights()
        {
            device.Material = mtrl;
            device.Lights[0].Type = LightType.Point;
            device.Lights[0].Diffuse = System.Drawing.Color.White;
            device.Lights[0].Range = 20.0f;
            device.Lights[0].Position = new Vector3(0, 2, -4);	//设置灯光位置,注意光线的方向
            device.Lights[0].Attenuation1 = 0.2f;
            device.Lights[0].Enabled = true;				//使设置有效 
            device.Lights[0].Update();					//更新灯光设置，创建第一盏灯光
            device.RenderState.Ambient = System.Drawing.Color.FromArgb(0x808080);

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeGraphics();
            this.Show();
            Render();
        }

    }
}
