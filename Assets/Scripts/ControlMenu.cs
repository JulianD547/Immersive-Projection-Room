using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.UI;

public class ControlMenu : MonoBehaviour
{
    public Camera SceneCamera;
    public InputField gameobject_input;

    //Position Input Fields
    public InputField xInputField_Pos;
    public InputField yInputField_Pos;
    public InputField zInputField_Pos;

    //Rotation Input Fields
    public InputField xInputField_Rot;
    public InputField yInputField_Rot;
    public InputField zInputField_Rot;

    //Scale Input Fields
    public InputField xInputField_Scale;
    public InputField yInputField_Scale;
    public InputField zInputField_Scale;

    //File Input Fields
    public InputField savetoFile;
    public InputField readFile;

    public GameObject modelQuad;
    public Material blackMaterial;

    public bool isTrace = false;

    public GameObject quad_Trace;

    public float x_value;

    public bool cubeVisible = true;

    //Lists to store points of a created cover
    private List<Vector3> innerPoints = new List<Vector3>();
    private List<Vector3> outerPoints = new List<Vector3>();

    // List to store the points
    private List<Vector3> points = new List<Vector3>();

    List<GameObject> trace_Points = new List<GameObject>();
    List<GameObject> active_Meshes = new List<GameObject>();
    List<GameObject> trace_Quads = new List<GameObject>();
    GameObject selectedObject;
    List<Vector3> vertices = new List<Vector3>();
    List<GameObject> verticeCubes = new List<GameObject>();
    public Material verticeMaterial;

    public GameObject cubePrefab;

    int atQuadNum = 0;
    Vector3[] quadPoints = new Vector3[4];
    List<GameObject> outerMask = new List<GameObject>();
    bool QMakeSlices = false;
    bool QMakeOuterMask = true;
    List<Vector3> outMaskPoints = new List<Vector3>();
    int numberOfMasks = 0;

    // Start is called before the first frame update
    void Start()
    {
        double[] xs = { 0, 1, 2, 3, 4 };
        double[] ys = { 0, 1, 0, 1, 0 };

        var spline = new CubicSpline(xs, ys);
        Debug.Log(spline.Evaluate(2.3));

        Debug.Log("displays connected: " + Display.displays.Length);
        // Display.displays[0] is the primary, default display and is always ON, so start at index 1.
        // Check if additional displays are available and activate each.

        for (int i = 1; i < Display.displays.Length; i++)
        {
            Display.displays[i].Activate();
        }

    }



    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (!isTrace)
            {
                //Vector2 v = new Vector2(Input.mousePosition.x-3440, Input.mousePosition.)
                // Perform a raycast from the mouse position
                Ray ray = SceneCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                //gameobject_input.text = Input.mousePosition.x.ToString();
                if (Physics.Raycast(ray, out hit))
                {
                    // Check if the GameObject hit by the raycast is this GameObject

                    // Display the GameObject's name
                    gameobject_input.text = hit.collider.gameObject.name;

                    //Set selectedObject and grab transform variables
                    selectedObject = hit.collider.gameObject;
                    Vector3 position = selectedObject.transform.position;
                    Vector3 rotation = selectedObject.transform.eulerAngles;
                    Vector3 localScale = selectedObject.transform.localScale;

                    // Update the input fields with the position values
                    xInputField_Pos.text = position.x.ToString();
                    yInputField_Pos.text = position.y.ToString();
                    zInputField_Pos.text = position.z.ToString();

                    // Update the input fields with the rotation values
                    xInputField_Rot.text = rotation.x.ToString();
                    yInputField_Rot.text = rotation.y.ToString();
                    zInputField_Rot.text = rotation.z.ToString();

                    // Update the input fields with the scale values
                    xInputField_Scale.text = localScale.x.ToString();
                    yInputField_Scale.text = localScale.y.ToString();
                    zInputField_Scale.text = localScale.z.ToString();

                    //showVertices
                    if (!selectedObject.name.Contains("vertice"))
                    {
                        showVertices(selectedObject, 0.1f, Color.blue);
                    }
                }
            }
            else
            {
                Ray ray = SceneCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    atQuadNum++;
                    if (QMakeSlices)
                    {
                        if (!hit.collider.gameObject.name.Contains("trace"))
                        {
                            GameObject newCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            newCube.transform.position = hit.point;
                            newCube.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
                            newCube.name = "trace_" + trace_Points.Count;
                            trace_Points.Add(newCube);
                            quadPoints[atQuadNum - 1] = hit.point;
                        }
                        else
                        {
                            quadPoints[atQuadNum - 1] = hit.point - new Vector3(0, 0, 0.025f);
                        }
                        if (atQuadNum == 4)
                        {
                            GameObject newQuad = Instantiate(modelQuad);
                            newQuad.GetComponent<MeshFilter>().mesh.vertices = quadPoints;
                            newQuad.GetComponent<MeshFilter>().mesh.RecalculateBounds();
                            trace_Quads.Add(newQuad);
                            atQuadNum = 0;
                        }
                    }
                    if (QMakeOuterMask)
                    {
                        GameObject newMaskCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        newMaskCube.transform.position = hit.point;
                        newMaskCube.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
                        newMaskCube.transform.name = "cube_" + newMaskCube.transform.position.x;
                        trace_Points.Add(newMaskCube);
                        outMaskPoints.Add(hit.point);
                    }
                }
            }
        }
    }


    public void DeleteLastCube()
    {
        if (trace_Points.Count > 0)
        {
            GameObject lastCube = trace_Points[trace_Points.Count - 1];
            trace_Points.RemoveAt(trace_Points.Count - 1);
            Destroy(lastCube);

            // Also remove the last point from outMaskPoints if applicable
            if (outMaskPoints.Count > 0)
            {
                outMaskPoints.RemoveAt(outMaskPoints.Count - 1);
            }
        }
    }

    public void ReadPointsFromFile(string path)
    {
        innerPoints.Clear();
        outerPoints.Clear();
        string currentType = "";

        if (!File.Exists(path))
        {
            Debug.LogError("File not found: " + path);
            return;
        }

        string[] lines = File.ReadAllLines(path);

        foreach (string line in lines)
        {
            if (line == "inner" || line == "outer")
            {
                currentType = line;
                continue;
            }

            string[] parts = line.Split(',');
            if (parts.Length == 3 && float.TryParse(parts[0], out float x) && float.TryParse(parts[1], out float y) && float.TryParse(parts[2], out float z))
            {
                Vector3 point = new Vector3(x, y, z);
                GameObject cubeInstance = Instantiate(cubePrefab, point, Quaternion.identity);
                cubeInstance.name = "Cube_" + point.x;
                cubeInstance.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);

                if (currentType == "inner")
                {
                    innerPoints.Add(point);
                }
                else if (currentType == "outer")
                {
                    outerPoints.Add(point);
                }
            }
        }
    }

    public void readQuad()
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            // Check if the object's name starts with "Cube_" and ensure it's part of the scene
            if (obj.name.StartsWith("Cube_") && obj.hideFlags == HideFlags.None && obj.scene.isLoaded)
            {
                Destroy(obj); // Destroy the object
            }
            else if (obj.name.StartsWith("modelQuad") && obj.hideFlags == HideFlags.None && obj.scene.isLoaded)
            {
                Destroy(obj);
            }
        }
        string userInput = readFile.text;
        string filePath = "Assets/" + userInput + ".txt"; // Path to the file

        // Read the file and parse the points based on type
        ReadPointsFromFile(filePath);

        // Process outer points if any
        if (outerPoints.Count > 0)
        {
            ProcessMask(outerPoints, "outer", userInput);
        }

        // Process inner points if any
        if (innerPoints.Count > 0)
        {
            ProcessMask(innerPoints, "inner", userInput);
        }

    }

    private void ProcessMask(List<Vector3> points, string type, string filename)
    {
        double[] xs = new double[points.Count];
        double[] ys = new double[points.Count];
        List<Vector3> interpolatedPoints = new List<Vector3>();

        for (int i = 0; i < points.Count; i++)
        {
            xs[i] = points[i].x;
            ys[i] = points[i].y;
        }

        var parametricSpline = new ParametricSpline(xs, ys);

        for (double t = 0; t <= 1; t += 0.01)
        {
            var (x, y) = parametricSpline.Evaluate(t);
            interpolatedPoints.Add(new Vector3((float)x, (float)y, points[0].z));
        }

        if (type == "outer")
        {
            var outerMask = computeOuterMask(interpolatedPoints, 2000f, filename, 2);
        }
        else if (type == "inner")
        {
            var innerMask = computeInnerMask(interpolatedPoints, filename, 2);
        }
    }

    public void OuterButtonClick()
    {

        string userInput = savetoFile.text;

        double[] xs = new double[outMaskPoints.Count];
        double[] ys = new double[outMaskPoints.Count];
        List<Vector3> interpolatedPoints = new List<Vector3>();
        for (int i = 0; i < outMaskPoints.Count; i++)
        {
            xs[i] = outMaskPoints[i].x;
            ys[i] = outMaskPoints[i].y;

        }


        var parametricSpline = new ParametricSpline(xs, ys);

        for (double t = 0; t <= 1; t += 0.01)
        {
            var (x, y) = parametricSpline.Evaluate(t);
            interpolatedPoints.Add(new Vector3((float)x, (float)y, outMaskPoints[0].z));
        }
        outerMask = computeOuterMask(interpolatedPoints, 2000f, userInput, 1);
    }

    public void InnerButtonClick()
    {

        string userInput = savetoFile.text;
        double[] xs = new double[outMaskPoints.Count];
        double[] ys = new double[outMaskPoints.Count];
        List<Vector3> interpolatedPoints = new List<Vector3>();
        for (int i = 0; i < outMaskPoints.Count; i++)
        {
            xs[i] = outMaskPoints[i].x;
            ys[i] = outMaskPoints[i].y;
        }

        var parametricSpline = new ParametricSpline(xs, ys);

        for (double t = 0; t <= 1; t += 0.01)
        {
            var (x, y) = parametricSpline.Evaluate(t);
            interpolatedPoints.Add(new Vector3((float)x, (float)y, outMaskPoints[0].z));
        }
        outerMask = computeInnerMask(interpolatedPoints, userInput, 1);
    }

    void saveFile(string fileName, List<Vector3> points, string type)
    {
        string filePath = "Assets/" + fileName + ".txt";
        List<string> fileLines = new List<string>();

        // Check if the filz already exists and read its contents if it does
        if (File.Exists(filePath))
        {
            fileLines = new List<string>(File.ReadAllLines(filePath));
            int startIndex = -1, endIndex = -1;

            // Determine if we need to clear existing 'outer' or 'inner' points
            for (int i = 0; i < fileLines.Count; i++)
            {
                if (fileLines[i].Trim() == type)
                {
                    startIndex = i; // mark the start of the section to clear
                    // Find the end of this section by searching for the next section or end of file
                    for (int j = i + 1; j < fileLines.Count; j++)
                    {
                        if (fileLines[j].Trim() == "outer" || fileLines[j].Trim() == "inner")
                        {
                            endIndex = j;
                            break;
                        }
                    }
                    if (endIndex == -1) { endIndex = fileLines.Count; } // if no other section found, clear till end
                    fileLines.RemoveRange(startIndex, endIndex - startIndex);
                    break;
                }
            }
        }

        // Rewrite the file with updated sections
        using (StreamWriter sw = new StreamWriter(filePath))
        {
            foreach (string line in fileLines)
            {
                sw.WriteLine(line); // Rewrite lines that are not cleared
            }

            // Now append the new points for the given type
            sw.WriteLine(type);
            foreach (Vector3 point in points)
            {
                sw.WriteLine($"{point.x},{point.y},{point.z}");
            }
        }
    }

    Vector3 computeCentroid(List<Vector3> points)
    {
        Vector3 sum = Vector3.zero;
        for (int i = 0; i < points.Count; i++)
        {
            sum += points[i];
        }
        sum = sum / points.Count;
        return sum;
    }

    List<GameObject> computeInnerMask(List<Vector3> points, string filename, int num)
    {

        List<GameObject> innerMask = new List<GameObject>();
        Vector3 centroid = computeCentroid(points);
        for (int i = 0; i < points.Count; i++)
        {
            /* GameObject oneTriangle = new GameObject();
             MeshFilter mf = oneTriangle.AddComponent<MeshFilter>();
            MeshRenderer mr= oneTriangle.AddComponent<MeshRenderer>();
             mr.material = blackMaterial;
             Vector3[] vertices = new Vector3[3];
             vertices[0] = centroid;
             vertices[1] = points[i];
             vertices[2] = points[(i + 1) % points.Count];
             Vector2[] uv = new Vector2[3];
             uv[0] = new Vector2(0, 0);
             uv[1] = new Vector2(1, 0);
             uv[2] = new Vector2(0, 1);
             Vector3[] normals = new Vector3[3];
             normals[0] = new Vector3(0, 0, 1);
             normals[1] = new Vector3(0, 0, 1);
             normals[2] = new Vector3(0, 0, 1);

             int[] triangle = new int[3];
             triangle[0] = 0;
             triangle[1] = 1;
             triangle[2] = 2;
             mf.mesh.vertices = vertices;
             mf.mesh.triangles = triangle;
             mf.mesh.uv = uv;
             mf.mesh.normals = normals;
             mf.mesh.RecalculateBounds();
             innerMask.Add(oneTriangle);
            */

            innerMask.Add(Instantiate(modelQuad));
            Vector3[] quadPoints = new Vector3[4];
            quadPoints[0] = centroid;
            quadPoints[1] = points[i % points.Count];
            quadPoints[2] = centroid;
            quadPoints[3] = points[(i + 1) % points.Count];
            innerMask[innerMask.Count - 1].GetComponent<MeshFilter>().mesh.vertices = quadPoints;
            innerMask[innerMask.Count - 1].GetComponent<MeshFilter>().mesh.RecalculateBounds();

        }
        numberOfMasks++;
        if (num == 1)
        {
            saveFile(filename, outMaskPoints, "inner");
            outMaskPoints.Clear();
        }

        return innerMask;

    }
    List<GameObject> computeOuterMask(List<Vector3> points, float percentExtrapolation, string filename, int num)
    {
        List<GameObject> outerMasks = new List<GameObject>();
        Vector3 centroid = computeCentroid(points);
        List<Vector3> extrapolatedPoints = new List<Vector3>();

        //compute the extrapolated points, think of this like an explosion from the center pushing the points outward

        for (int i = 0; i < points.Count; i++)
        {
            Vector3 ex = centroid + percentExtrapolation / 100f * (points[i] - centroid);
            extrapolatedPoints.Add(ex);
        }

        //create quads with black color to mask

        for (int i = 0; i < points.Count; i++)
        {
            outerMasks.Add(Instantiate(modelQuad));
            Vector3[] quadPoints = new Vector3[4];
            quadPoints[0] = points[i % points.Count];
            quadPoints[1] = extrapolatedPoints[i % points.Count];
            quadPoints[2] = points[(i + 1) % points.Count];
            quadPoints[3] = extrapolatedPoints[(i + 1) % points.Count];
            outerMasks[outerMasks.Count - 1].GetComponent<MeshFilter>().mesh.vertices = quadPoints;
            outerMasks[outerMasks.Count - 1].GetComponent<MeshFilter>().mesh.RecalculateBounds();

        }

        numberOfMasks++;
        if (num == 1)
        {
            saveFile(filename, outMaskPoints, "outer");
            outMaskPoints.Clear();
        }

        return outerMasks;
    }

    public void startTrace()
    {

        isTrace = !isTrace;
        if (isTrace)
        {
            GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
            trace_Points.Clear();
            active_Meshes.Clear();
            for (int i = 0; i < allObjects.Length; i++)
            {

                if (allObjects[i].GetComponent<MeshFilter>() != null)
                {

                    active_Meshes.Add(allObjects[i]);
                    allObjects[i].SetActive(false);
                }
            }
            quad_Trace.SetActive(true);
        }
        else
        {
            for (int i = 0; i < active_Meshes.Count; i++)
            {
                active_Meshes[i].SetActive(true);
            }
            quad_Trace.SetActive(false);
        }
    }
    public void UpdateTransform()
    {
        // Check if a GameObject is selected
        if (selectedObject != null)
        {
            // Parse the input field values
            float x_Pos = float.Parse(xInputField_Pos.text);
            float y_Pos = float.Parse(yInputField_Pos.text);
            float z_Pos = float.Parse(zInputField_Pos.text);

            // Update the position of the selected GameObject
            selectedObject.transform.position = new Vector3(x_Pos, y_Pos, z_Pos);

            // Parse the input field values
            float x_Rot = float.Parse(xInputField_Rot.text);
            float y_Rot = float.Parse(yInputField_Rot.text);
            float z_Rot = float.Parse(zInputField_Rot.text);

            // Update the rotation of the selected GameObject
            selectedObject.transform.eulerAngles = new Vector3(x_Rot, y_Rot, z_Rot);

            // Parse the input field values
            float x_Scale = float.Parse(xInputField_Scale.text);
            float y_Scale = float.Parse(yInputField_Scale.text);
            float z_Scale = float.Parse(zInputField_Scale.text);

            // Update the scale of the selected GameObject
            selectedObject.transform.localScale = new Vector3(x_Scale, y_Scale, z_Scale);
            if (selectedObject.name.Contains("vertice"))
            {
                string parentname = selectedObject.name.Split('_')[0];
                GameObject myparent = GameObject.Find(parentname);
                Vector3[] vertices = myparent.GetComponent<MeshFilter>().mesh.vertices;
                int vertice_num = Convert.ToInt16(selectedObject.name.Split('_')[2]);
                vertices[vertice_num] = new Vector3(x_Pos, y_Pos, z_Pos) - myparent.transform.position;
                myparent.GetComponent<MeshFilter>().mesh.vertices = vertices;
                myparent.GetComponent<MeshFilter>().mesh.RecalculateBounds();


            }

        }
    }

    void showVertices(GameObject go, float cubeSize, Color myColor)
    {
        MeshFilter mf = go.GetComponent<MeshFilter>();
        Mesh mesh = mf.mesh;
        Vector3[] myvertices = mesh.vertices;

        for (int i = 0; i < myvertices.Length; i++)
        {
            vertices.Add(myvertices[i]);
            verticeCubes.Add(GameObject.CreatePrimitive(PrimitiveType.Cube));
            verticeCubes[verticeCubes.Count - 1].transform.localScale = new Vector3(cubeSize, cubeSize, cubeSize);
            verticeCubes[verticeCubes.Count - 1].transform.position = go.transform.position + vertices[i];
            verticeMaterial.color = myColor;
            verticeCubes[verticeCubes.Count - 1].GetComponent<MeshRenderer>().material = verticeMaterial;
            verticeCubes[verticeCubes.Count - 1].name = go.name + "_vertice_" + i.ToString();


        }
    }
    public float UpdatePositionInFile(GameObject gameObject, float x_value)
    {
        string userInput = savetoFile.text;
        string filePath = "Assets/" + userInput + ".txt";

        float newX = gameObject.transform.position.x;
        Debug.Log(newX);
        float newY = gameObject.transform.position.y;
        float newZ = gameObject.transform.position.z;

        List<string> lines = new List<string>();
        bool updated = false;

        // Read all lines from the file
        if (File.Exists(filePath))
        {
            lines = new List<string>(File.ReadAllLines(filePath));

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Contains("outer") || lines[i].Contains("inner"))
                {
                    continue; // Skip lines that contain "outer" or "inner"
                }

                // Process lines expected to have position data
                string[] parts = lines[i].Split(',');
                if (parts.Length == 3 && float.TryParse(parts[0], out float xPos))
                {
                    Debug.Log("a" + xPos + x_value);
                    if (Mathf.Approximately(xPos, x_value)) // Check if the X positions match
                    {
                        Debug.Log("b" + xPos + x_value);
                        // Update the line with new XYZ position
                        lines[i] = $"{newX},{newY},{newZ}";

                        updated = true;
                        break; // Remove break if you want to update all matching X positions
                    }
                }
            }

            // Write the updated data back to the file only if an update was necessary
            if (updated)
            {
                File.WriteAllLines(filePath, lines);
            }
        }
        else
        {
            Debug.LogError("File not found: " + filePath);
        }


        return newX;
    }

    public void updatePosition()
    {
        x_value = float.Parse(selectedObject.name.Split('_')[1]);
        Debug.Log("x_value" + x_value);
        float new_x = UpdatePositionInFile(selectedObject, x_value);
        selectedObject.name = "cube_" + new_x;
        readQuad();


    }

    public void upButton()
    {
        selectedObject.transform.position = new Vector3(selectedObject.transform.position.x, selectedObject.transform.position.y + 0.1f, selectedObject.transform.position.z);


    }

    public void downButton()
    {
        selectedObject.transform.position = new Vector3(selectedObject.transform.position.x, selectedObject.transform.position.y - 0.1f, selectedObject.transform.position.z);

    }

    public void leftButton()
    {
        selectedObject.transform.position = new Vector3(selectedObject.transform.position.x - 0.1f, selectedObject.transform.position.y, selectedObject.transform.position.z);

    }

    public void rightButton()
    {
        selectedObject.transform.position = new Vector3(selectedObject.transform.position.x + 0.1f, selectedObject.transform.position.y, selectedObject.transform.position.z);

    }

    public void QuitProgram()
    {
        Application.Quit();
    }

    public void cubeVisibility()
    {
        cubeVisible = !cubeVisible;
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.name.StartsWith("Cube_") && obj.hideFlags == HideFlags.None && obj.scene.isLoaded)
            {
                obj.SetActive(cubeVisible);
            }
        }

    }
}

