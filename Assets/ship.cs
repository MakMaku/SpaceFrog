using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ship : MonoBehaviour
{
    public float K = 0f;
    public float m = 1000f;
    public GameObject planet;           //Íåáåñíûé îáúåêò, ê êîòîðîìó áóäåò ëåòåòü êîðàáëü
    public float Ft = 20000f;           //Ñèëà òÿãè
    public float V = 0f;

    private List<float> V0;             //Âåêòîð ñîñòîÿíèÿ (V0)
    public List<float> C;
    public float t = 0, cur_R;
    private Sun Sun_cs;
    private Planet Planet_cs;
    private Sputnik Sputnik_cs;
    private string[] tags = { "Star", "Planet", "Sputnik" };

    /*GameObject Cloth_Cel(){             //íàõîæäåíèå áëèæàéùåãî îáúåêòà
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (string name in tags){
            GameObject[] Cel_Obj = GameObject.FindGameObjectsWithTag(name);
            foreach (GameObject go in Cel_Obj){
                Vector3 diff = go.transform.position - position;
                float curDistance = diff.sqrMagnitude;
                if (curDistance < distance){
                    Closest = go;
                    distance = curDistance;
                }
            }
        }
        return Closest;
    }*/

    List<float> RightParts(List <float> X, float Course){
        List<float> temp = new List<float> { 0, 0, 0, 0 }; //dx/dt, dy/dt, dVx/dt, dVy/dt
        temp[0] = X[2] * Mathf.Cos(Course);      // dx/dt
        temp[1] = X[3] * Mathf.Sin(Course);      // dy/dt
        
        var Mass = 0f;
        float Fx = 0f, Fy=0f;
        //Íàõîæäåíèå ðåçóëüòèðóþùåé ñèëû òÿãîòåíèÿ
        foreach (string name in tags) {
            GameObject[] Cel_Obj = GameObject.FindGameObjectsWithTag(name);
            foreach (GameObject go in Cel_Obj) {
                if (go.tag == tags[0]) {
                    Sun_cs = GameObject.Find(go.name).GetComponent<Sun>();
                    Mass = Sun_cs.Mass;
                }
                else if (go.tag == tags[1]) {
                    Planet_cs = GameObject.Find(go.name).GetComponent<Planet>();
                    Mass = Planet_cs.Mass;
                }
                else if (go.tag == tags[2]) {
                    Sputnik_cs = GameObject.Find(go.name).GetComponent<Sputnik>();
                    Mass = Sputnik_cs.Mass;
                }
                var R = Mathf.Sqrt(Mathf.Pow(go.transform.position.x - transform.position.x,2)+ Mathf.Pow(go.transform.position.y - transform.position.y,2));
                Fx = Fx + Mass * (go.transform.position.x - transform.position.x) / Mathf.Pow(R, 3);
                Fy = Fy + Mass * (go.transform.position.y - transform.position.y) / Mathf.Pow(R, 3);
            }
        }
        temp[2] = Ft / m * Mathf.Cos(Course) + Fx;    // dVx/dt
        temp[3] = Ft / m * Mathf.Sin(Course) + Fy;    // dVy/dt
        return temp;
    }


    List<float> Find_Course(){
        List <float> Course= new List<float> { 0,0,0,0};
       
        //âåêòîð ñîòîÿíèÿ êîðîáëÿ
        List<float> X_sh = new List<float> { transform.position.x, transform.position.y, 0, 0};

        //Ïàðàìåòðû ïëàíåòû
            //Ðàäèóñ îðáèòû
        float radius = Mathf.Sqrt(Mathf.Pow(planet.transform.position.x, 2) + Mathf.Pow(planet.transform.position.y, 2));
        Transform parent = planet.transform.parent;
        Sun_cs = GameObject.Find(planet.transform.parent.gameObject.name).GetComponent<Sun>();

            //Ñêîðîñòü ïëàíåòû
        float speed = Mathf.Sqrt(Sun_cs.Mass) / Mathf.Sqrt(Mathf.Pow(radius, 3));

            //Âåêòîð ïîëîæåíèÿ ïëàíåòû
        Vector2 pos_co = new Vector2();
        Vector2 cashedCenter = new Vector2(parent.position.x, parent.position.y) + new Vector2(radius, 0);

        //Ðàññòîÿíèå îò êîðàáëÿ äî öåíòðà ñèñòåìû (îðáèòû)
        var Rac = Mathf.Sqrt(Mathf.Pow(X_sh[0], 2) + Mathf.Pow(X_sh[1], 2));

        //StreamWriter sw;
        //Êîîðäèíàòû áëèæàéøåé òî÷êè îðáèòû
        float xb = X_sh[0] * (radius / Rac);
        float yb = X_sh[1] * (radius / Rac);

        //Êóðñ äî òî÷êè (xb, yb)
        Course[0] = Mathf.Atan2((yb- X_sh[1]) , (xb- X_sh[0]));
        
        float pogreshnost = 1000f, pog_time=2f;

        //delta_sh - âðåìÿ çà êîòîðîå ïðèëåòèò êîðàáëü â òî÷êó (xb, yb)
        //delta_co - âðåìÿ çà êîòîðîå ïðèëåòèò ïëàíåòà â òî÷êó (xb, yb)
        float delta_sh = 0 ,delta_co=0;
        StreamWriter sw = new StreamWriter("Course.txt", false); sw.Close();
        sw = new StreamWriter("Time.txt", false); sw.Close();
        //ïàðàìåòð çàäàíèÿ òî÷êè (xb, yb)
        float alpha = Mathf.Acos(xb / radius)*Mathf.Rad2Deg;

        float old_R = 0f, new_R = Mathf.Sqrt(Mathf.Pow((X_sh[0] - xb), 2) + Mathf.Pow((X_sh[1] - yb), 2));
        do {
            
            //öèêë èçìåðåíèÿ delta_sh
            do {
                old_R = new_R;
                for (int i = 0; i < 4; i++)
                    X_sh[i] = X_sh[i] + RightParts(X_sh, Course[0])[i] * Time.deltaTime;
                new_R = Mathf.Sqrt(Mathf.Pow((X_sh[0] - xb), 2) + Mathf.Pow((X_sh[1] - yb), 2));
                delta_sh += Time.deltaTime;
            } while (new_R<old_R);
            
            //öèêë èçìåíåíèÿ delta_co
            do {
                delta_co += Time.deltaTime;
                var x = Mathf.Cos(delta_co * speed) * radius;
                var y = Mathf.Sin(delta_co * speed) * radius;
                pos_co = new Vector2(x, y) + cashedCenter - new Vector2(radius, 0);
            } while ((Mathf.Abs(pos_co.x - xb) >= pogreshnost) || (Mathf.Abs(pos_co.y - yb) >= pogreshnost));

            //Óñëîâèå âûõîäà èç öèêëà
            if (Mathf.Abs(delta_sh - delta_co) <= pog_time) {
                Course[0] = Mathf.Atan2(yb - X_sh[1], xb - X_sh[0]);
                Course[1] = new_R;
                Course[2] = xb;
                Course[3] = yb;
                break;
            }

            //Ïîðÿäîê äåéòñâèé ïðè ïðîäîëæåíèè öèêëà
            if (Mathf.Abs(delta_sh - delta_co) > pog_time) {
                alpha -= 1f;
                xb = radius * Mathf.Cos(alpha * Mathf.Deg2Rad) + cashedCenter.x - radius;
                yb = radius * Mathf.Sin(alpha * Mathf.Deg2Rad) + cashedCenter.y;
                Course[0] = Mathf.Atan2(yb - X_sh[1], xb - X_sh[0]);
                delta_sh = delta_co = 0;
                X_sh= V0;
                new_R = Mathf.Sqrt(Mathf.Pow((X_sh[0] - xb), 2) + Mathf.Pow((X_sh[1] - yb), 2));
            }
        } while (Mathf.Abs(delta_sh-delta_co)<pog_time); 
        return Course;
    }

    
    // Start is called before the first frame update
    
    void Start() {
        V0 = new List<float> { transform.position.x, transform.position.y, 0, 0 };
        C = Find_Course(); //0 - Course, 1 - crit_R, 2 - xb, 3 - yb
    }

    // Update is called once per frame
    void Update(){
        var cur_R = Mathf.Sqrt(Mathf.Pow((V0[0] - C[2]), 2) + Mathf.Pow((V0[1] - C[3]), 2));
        if (cur_R <= C[1]) {
            C[0] = Mathf.Atan2((C[3] - V0[1]), (C[2] - V0[0]));
        }
        for (int i = V0.Count - 1; i >= 0; i--) {
            V0[i] += RightParts(V0, C[0])[i] * Time.deltaTime;                     //Èíòåãðèðîâàíèå
        }
        t += Time.deltaTime;                                                       //ìåòîäîì Ýéëåðà
        transform.rotation = Quaternion.Euler(0, 0, C[0] * Mathf.Rad2Deg + 90);    //Поворот корабля
        V = Mathf.Sqrt(V0[2] * V0[2] + V0[3] * V0[3]);
        transform.position = new Vector2(V0[0], V0[1]);
    }
}
