using UnityEngine;

// Вспомогательный класс: короткая вспышка частиц (подбор монеты, взрыв, смерть)
public static class Effects
{
    // материал для частиц задается из GameManager
    public static Material ParticleMaterial;

    public static void Burst(Vector3 position, Color color, int count, float speed)
    {
        var go = new GameObject("Burst");
        go.transform.position = position;

        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.duration = 0.5f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.4f, speed);
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.35f);
        main.startColor = color;
        main.gravityModifier = 0.5f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.stopAction = ParticleSystemStopAction.Destroy; // сам удалится

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        if (ParticleMaterial != null) renderer.sharedMaterial = ParticleMaterial;

        ps.Play();
    }
}
