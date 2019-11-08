using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Partical : MonoBehaviour {
	private ParticleSystem particleSys;  // 粒子系统  
	private ParticleSystem.Particle[] particleArr;  // 粒子数组  
	private CirclePosition[] circle;  
	public int count = 5000;    //发射的最大粒子数     
	public float size = 0.05f;       //粒子的大小  
	public float minRadius = 5.0f;   //粒子的旋转半径
	public float maxRadius = 10.0f;  
	public bool clockwise = true;    // 顺时针|逆时针
	public float speed = 5f;          //粒子的运动速度
	public float pingPong = 0.05f;  // 粒子在半径方向上游离的范围
	public Gradient colorGradient;  //控制渐变
	// Use this for initialization
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
}

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
