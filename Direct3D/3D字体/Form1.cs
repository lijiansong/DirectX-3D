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

namespace _3D字体
{
    public partial class Form1 : Form
    {
        private Device device = null;
        bool pause = false;
        Mesh mesh = null;
        Material meshMaterials;
        float Angle = 0, ViewZ = -5.0f;

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
            System.Drawing.Font currentFont = new System.Drawing.Font("Arial", 15);
            mesh = Mesh.TextFromFont(device, currentFont, "我的3D字体", 0.001f, 0.5f);
            meshMaterials = new Material();
            meshMaterials.Ambient = System.Drawing.Color.White;		//材质如何反射环境光
            meshMaterials.Diffuse = System.Drawing.Color.White;		//材质如何反射灯光


        }

        public void OnResetDevice(object sender, EventArgs e)
        {
            Device dev = (Device)sender;
            dev.RenderState.ZBufferEnable = true;		 	//允许使用深度缓冲
            dev.RenderState.Ambient = Color.Black;	 	 	//环境光为深蓝色
            dev.Lights[0].Type = LightType.Directional;  	//设置灯光类型
            dev.Lights[0].Diffuse = Color.White;			//设置灯光颜色
            dev.Lights[0].Direction = new Vector3(0, -1, 1);	//设置灯光位置
            dev.Lights[0].Update();						//更新灯光设置，创建第一盏灯光
            dev.Lights[0].Enabled = true;					//使设置有效
            dev.Material = meshMaterials;					//指定设备的材质

        }

        public void Render()		//渲染方法，本方法没有任何渲染代码，可认为是渲染方法的框架
        {
            if (device == null) 			//如果未建立设备对象，退出
                return;
            if (pause)
                return;
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, System.Drawing.Color.Blue, 1.0f, 0);
            device.BeginScene();			//开始渲染
            SetupMatrices();				//矩阵变换
            mesh.DrawSubset(0);			//显示茶壶，见5.12.6节
            device.EndScene();			//渲染结束
            device.Present();	//更新显示区域，把后备缓存的3D图形送到图形卡的屏幕显示区中显示

        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            this.Render();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            pause = ((this.WindowState == FormWindowState.Minimized) || !this.Visible);
        }

        void SetupMatrices()
        {
            device.Transform.World = Matrix.Translation(-2.5f, 0, 0) * //世界变换矩阵
Matrix.RotationYawPitchRoll(Angle, Angle, 0);
            device.Transform.View = Matrix.LookAtLH(new Vector3(0.0f, 1.0f, ViewZ),//观察变换矩阵
         new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f));
            device.Transform.Projection =		//投影变换矩阵
                             Matrix.PerspectiveFovLH((float)(Math.PI / 4), 1.33f, 1.0f, 100.0f);

        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)				//e.KeyCode是键盘每个键的编号
            {
                case Keys.Left:			//Keys.Left是左箭头键编号，茶壶左移
                    Angle += 0.1F;
                    break;
                case Keys.Right:			//茶壶右移
                    Angle -= 0.1F;
                    break;
                case Keys.Down:			//茶壶下移
                    ViewZ += 1;
                    break;
                case Keys.Up:				//茶壶上移
                    ViewZ -= 1;
                    break;
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeGraphics();
            this.Show();
            Render();
        }
    }
}
