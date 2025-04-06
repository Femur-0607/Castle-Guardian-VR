using UnityEngine;

public class MCPTest : MonoBehaviour
{
    public string testMessage = "MCP Test Successful!";
    
    void Start()
    {
        Debug.Log(testMessage);
        Debug.Log("This is a direct test message from MCPTest script!");
        TestMethod();
    }
    
    public void TestMethod()
    {
        Debug.Log("Test method called successfully!");
    }
}
