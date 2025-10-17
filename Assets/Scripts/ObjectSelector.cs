using UnityEngine;

public class ObjectSelector : MonoBehaviour
{
    [System.Serializable]
    public class PushableObject
    {
        public string name;
        public GameObject prefab;
        public float mass;
        public float staticFriction;
        public float kineticFriction;
    }
    
    public PushableObject[] objects;
    public Transform spawnPoint;
    public PushCube pushCube;
    
    private GameObject currentObject;
    
    public void SelectObject(int index)
    {
        if (index < 0 || index >= objects.Length) return;
        
        // Удаляем текущий объект
        if (currentObject != null)
        {
            Destroy(currentObject);
        }
        
        // Создаем новый объект
        currentObject = Instantiate(objects[index].prefab, spawnPoint.position, spawnPoint.rotation);
        
        // Настраиваем физические параметры
        var selected = objects[index];
        pushCube.SetMass(selected.mass);
        pushCube.SetStaticFriction(selected.staticFriction);
        pushCube.SetKineticFriction(selected.kineticFriction);
        
        // Устанавливаем ссылку на Rigidbody
        pushCube.objectRb = currentObject.GetComponent<Rigidbody>();
        
        pushCube.ResetObject();
    }
}