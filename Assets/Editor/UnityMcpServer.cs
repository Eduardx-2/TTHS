using System;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class UnityMcpServer
{
    private static HttpListener listener;
    private static readonly ConcurrentQueue<Action> mainThreadQueue = new ConcurrentQueue<Action>();
    private const string Url = "http://localhost:8080/";

    static UnityMcpServer()
    {
        EditorApplication.update += Update;
        StartServer();
    }

    public static void StartServer()
    {
        StopServer();
        try
        {
            listener = new HttpListener();
            listener.Prefixes.Add(Url);
            listener.Start();
            listener.BeginGetContext(OnRequest, null);
            Debug.Log($"<color=green>[MCP Server]</color> Escuchando en {Url}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[MCP Server] Error al iniciar: {ex.Message}");
        }
    }

    public static void StopServer()
    {
        if (listener != null && listener.IsListening)
        {
            listener.Stop();
            listener.Close();
            listener = null;
        }
    }

    private static void Update()
    {
        while (mainThreadQueue.TryDequeue(out var action))
        {
            action?.Invoke();
        }
    }

    private static void OnRequest(IAsyncResult result)
    {
        if (listener == null || !listener.IsListening) return;

        HttpListenerContext context = listener.EndGetContext(result);
        listener.BeginGetContext(OnRequest, null);

        mainThreadQueue.Enqueue(() => HandleRequest(context));
    }

    private static void HandleRequest(HttpListenerContext context)
    {
        string response = "";
        string path = context.Request.Url.AbsolutePath;

        if (path == "/hierarchy")
        {
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            var sb = new StringBuilder();
            foreach (var go in roots)
            {
                sb.AppendLine($"- {go.name} (Activo: {go.activeSelf}, Componentes: {go.GetComponents<Component>().Length})");
            }
            response = sb.ToString();
        }
        else
        {
            response = "Endpoint no encontrado.";
        }

        byte[] buffer = Encoding.UTF8.GetBytes(response);
        context.Response.ContentLength64 = buffer.Length;
        context.Response.ContentType = "text/plain; charset=utf-8";
        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
        context.Response.OutputStream.Close();
    }
}