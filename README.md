# 3D Game Programming & Design：粒子系统

## 粒子系统概述
> 粒子系统是模拟一些不确定、流动现象的技术。它采用许多形状简单且赋予生命的微小粒子作为基本元素来表示物体(一般由点或很小的多边形通过纹理贴图表示)，表达物体的总体形态和特征的动态变化。人们经常使用粒子系统模拟的现象有火、爆炸、烟、水流、火花、落叶、云、雾、雪、尘、流星尾迹或者象发光轨迹这样的抽象视觉效果等等。

**1、粒子（Particle）**

粒子是粒子系统管理的基本单位。一般它是材料（Material）。材料包含两个内容，纹理（texture）、shader，分别负责形态、光照效果、两个方面。通常，粒子系统包含基础材料库供用户选择。

**2、渲染（render）**

渲染是定义粒子材料与摄像机之间的关系。主要包括材料面方位、显示顺序、光照等信息

如果是 3D 渲染，需要 uv 贴图网格

**3、运动管理**

粒子运动学或物理学管理。包括颜色、力场、速度、碰撞等属性

**4、发射器管理**

粒子工厂的管理。包括粒子初始化、喷射位置与形状等等

## 作业要求
本次作业基本要求是三选一

1、简单粒子制作

- 按参考资源要求，制作一个粒子系统，参考资源
- 使用 3.3 节介绍，用代码控制使之在不同场景下效果不一样

2、完善官方的“汽车尾气”模拟

- 使用官方资源资源 Vehicle 的 car， 使用 Smoke 粒子系统模拟启动发动、运行、故障等场景效果

3、参考 http://i-remember.fr/en 这类网站，使用粒子流编程控制制作一些效果， 如“粒子光环”

- 可参考以前作业

##  基本配置
新建空对象myParticle，给myParticle新建两个空对象outer和inter，分别表示内层光环和外层光环，分别选择AddComponent->Effects->Particle System。这里需要给内环和外环的脚本是一样的都是脚本Particle.cs，只是需要调整一下参数。光环的两层采用不同方向的循环旋转，外环顺时针旋转，内环逆时针旋转。除此以外，每个粒子都会游离，而不是简单的转圈。

![在这里插入图片描述](https://img-blog.csdnimg.cn/20191108153125348.jpeg?x-oss-process=image/watermark,type_ZmFuZ3poZW5naGVpdGk,shadow_10,text_aHR0cHM6Ly9ibG9nLmNzZG4ubmV0L3dlaXhpbl80MDM3NzY5MQ==,size_16,color_FFFFFF,t_70)

## 代码分析

本次作业参考了师兄的[博客](https://blog.csdn.net/simba_scorpio/article/details/51251126)

- 首先声明私有变量粒子系统，粒子数组以及粒子极坐标的数组。
```javascript
public class CirclePosition  
private ParticleSystem particleSys;  // 粒子系统  
private ParticleSystem.Particle[] particleArr;  // 粒子数组  
private CirclePosition[] circle;  
```
-  结构CirclePosition，用来记录每个粒子当前的半径、角度和游离起始时间。
```javascript
public class CirclePosition  
{  
	public float radius = 0f, angle = 0f, time = 0f;  
	public CirclePosition(float radius, float angle, float time)  
	{  
		this.radius = radius;    
		this.angle = angle;      
		this.time = time;        
	}  
}  
```
- 提供下面的一些public变量，使得可以在外部通过更改这些参数来设定光环的属性。
```javascript
	public int count = 5000;    //发射的最大粒子数     
	public float size = 0.05f;       //粒子的大小  
	public float minRadius = 5.0f;   //粒子的旋转半径
	public float maxRadius = 10.0f;  
	public bool clockwise = true;    // 顺时针|逆时针
	public float speed = 5f;          //粒子的运动速度
	public float pingPong = 0.05f;  // 粒子在半径方向上游离的范围
	public Gradient colorGradient;  //控制渐变
```
-   之后可以对粒子系统进行初始化了，开始的时候需要设置粒子发射器的参数，因为粒子的运动全部由程序实现，所以要把粒子的初始速度设置为0。
```javascript
	void Start () {
		particleArr = new ParticleSystem.Particle[count];  
		circle = new CirclePosition[count];  

		// 初始化粒子系统  
		particleSys = this.GetComponent<ParticleSystem>();  
		particleSys.startSpeed = 0;            // 粒子位置由程序控制  
		particleSys.startSize = size;          // 设置粒子大小  
		particleSys.loop = false;  
		particleSys.maxParticles = count;      // 设置最大粒子量  
		particleSys.Emit(count);               // 发射粒子  
		particleSys.GetParticles(particleArr);   

		RandomlySpread();   // 初始化各粒子位置 
	}
```
-  RandomlySpread将所有的粒子随机分布在圆圈轨道上。所有粒子运动由脚本控制，使用参数方程 x = cos(t), y = sin(t) 计算粒子位置，其中t是角度。
```javascript
	void RandomlySpread()  
	{  
		for (int i = 0; i < count; ++i)  
		{   // 随机每个粒子距离中心的半径，同时希望粒子集中在平均半径附近  
			float midRadius = (maxRadius + minRadius) / 2;  
			float minRate = Random.Range(1.0f, midRadius / minRadius);  
			float maxRate = Random.Range(midRadius / maxRadius, 1.0f);  
			float radius = Random.Range(minRadius * minRate, maxRadius * maxRate);  

			// 随机每个粒子的角度  
			float angle = Random.Range(0.0f, 360.0f);  
			float theta = angle / 180 * Mathf.PI;  

			// 随机每个粒子的游离起始时间  
			float time = Random.Range(0.0f, 360.0f);  

			circle[i] = new CirclePosition(radius, angle, time);  

			particleArr[i].position = new Vector3(circle[i].radius * Mathf.Cos(theta), 0f, circle[i].radius * Mathf.Sin(theta)); 
		}  

		particleSys.SetParticles(particleArr, particleArr.Length);  
	} 
```
- 在update函数中，用极坐标控制每个粒子的位置，保证每个粒子在平均半径附近。这样就基本上固定了粒子的位置是一个圆，然后让每个粒子动起来，绕着圆旋转，PingPong函数使得粒子可以在最大半径和最小半径之间游动。顺时针还是逆时针旋转的设置，也是在update中完成。
```javascript
	void Update () {
		for (int i = 0; i < count; i++)  
		{  
			if (clockwise)  // 顺时针旋转  
				circle[i].angle -= Time.deltaTime * speed / 10; 
			else            // 逆时针旋转  
				circle[i].angle += Time.deltaTime * speed / 10;  

			// 保证angle在0~360度  
			circle[i].angle = (360.0f + circle[i].angle) % 360.0f;  
			circle[i].time += Time.deltaTime;  
			circle[i].radius += Mathf.PingPong(circle[i].time / minRadius / maxRadius, pingPong) - pingPong / 2.0f; 
			float theta = circle[i].angle / 180 * Mathf.PI;  

			particleArr[i].position = new Vector3(circle[i].radius * Mathf.Cos(theta), 0f, circle[i].radius * Mathf.Sin(theta)); 
		}  
		changeColor ();
		particleSys.SetParticles(particleArr, particleArr.Length);  
	}
```
- 函数changeColor使粒子在运动过程中可以改变颜色，但是这里调用了colorGradient.Evaluate (value)这个函数传入时间来控制颜色的变化，因此需要我们在UI界面中调整两个粒子光环的渐变颜色范围。
```javascript
	void changeColor () {
		for (int i = 0; i < (int)Mathf.Sqrt(count); i++) {
			for (int j = 0; j < (int)Mathf.Sqrt (count); j++) {
				float value = (Time.realtimeSinceStartup - Mathf.Floor (Time.realtimeSinceStartup));
				value += circle [(int)(i*Mathf.Sqrt(count) + j)].angle / 2 / Mathf.PI;
				while (value > 1)
					value--;
				particleArr [(int)(i*Mathf.Sqrt(count) + j)].color = colorGradient.Evaluate (value);
			}
		}
	}
```
![在这里插入图片描述](https://img-blog.csdnimg.cn/20191108172111940.png?x-oss-process=image/watermark,type_ZmFuZ3poZW5naGVpdGk,shadow_10,text_aHR0cHM6Ly9ibG9nLmNzZG4ubmV0L3dlaXhpbl80MDM3NzY5MQ==,size_16,color_FFFFFF,t_70)	
## 游戏界面效果
![在这里插入图片描述](https://img-blog.csdnimg.cn/201911081722082.png?x-oss-process=image/watermark,type_ZmFuZ3poZW5naGVpdGk,shadow_10,text_aHR0cHM6Ly9ibG9nLmNzZG4ubmV0L3dlaXhpbl80MDM3NzY5MQ==,size_16,color_FFFFFF,t_70)

- 最后
[视频链接](https://pan.baidu.com/s/1-QyiGznEf8xQyf-mttx9Mw)

