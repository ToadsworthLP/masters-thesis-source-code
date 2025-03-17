#if TOOLS
using System;
using Godot;
using Umbra.Nodes;

namespace Umbra.Editor;

public partial class UmbraCompanionInspectorPlugin : EditorInspectorPlugin
{
    public override bool _CanHandle(GodotObject obj)
    {
        return obj != null && CompanionClassMap.HasCompanionType(obj.GetType());
    }

    public override void _ParseBegin(GodotObject obj)
    {
        Node targetNode = (Node)obj;
        if (!targetNode.HasCompanion())
        {
            Button addCompanionButton;
            addCompanionButton = new Button();
            addCompanionButton.Text = "Create Umbra Companion";
            addCompanionButton.Pressed += () =>
            {
                UmbraRoot umbraRoot = UmbraNodeUtils.FindUmbraRootInChildren(EditorInterface.Singleton.GetEditedSceneRoot());
                Type companionType = CompanionClassMap.GetCompanionType(EditorInterface.Singleton.GetInspector().GetEditedObject().GetType());
                Node targetNode = (Node)EditorInterface.Singleton.GetInspector().GetEditedObject();

                Node3D companionNode = (Node3D)Activator.CreateInstance(companionType);
                companionNode.Name = $"{targetNode.Name}Companion";
                umbraRoot.AddChild(companionNode);
                companionNode.Owner = EditorInterface.Singleton.GetEditedSceneRoot();
                
                targetNode.SetCompanion(companionNode);
                if(!targetNode.HasHeight()) targetNode.SetHeight(0f);
                ((IUmbraCompanion)companionNode).SetCompanion(targetNode);
            };
            AddCustomControl(addCompanionButton);
        }
        else
        {
            NodePath companionNodePath = targetNode.GetCompanion();
            if (!targetNode.HasNode(companionNodePath))
            {
                targetNode.RemoveCompanion();
                return;
            }
            
            IUmbraCompanion companion = targetNode.GetNode<IUmbraCompanion>(companionNodePath);

            PackedScene editorPackedScene = GD.Load<PackedScene>("res://addons/Umbra/Scenes/Scn_Umbra_CompanionEditor.tscn");
            Control editorScene = editorPackedScene.Instantiate<Control>();

            MenuButton companionButton = editorScene.GetNode<MenuButton>("MarginContainer/Properties/CompanionLine/MenuButton");
            Node companionNode = (Node)companion;
            companionButton.Text = companionNode.Name;
            PopupMenu popupMenu = companionButton.GetPopup();

            Callable popupOptionSelect = Callable.From((long id) =>
            {
                Node targetNode = (Node)EditorInterface.Singleton.GetInspector().GetEditedObject();

                switch (id)
                {
                    // Select
                    case 0:
                        EditorInterface.Singleton.EditNode(targetNode.GetNode(targetNode.GetCompanion()));
                        break;
                    // Unlink
                    case 1:
                        targetNode.RemoveCompanion();
                        break;
                }
            });
            
            popupMenu.Connect(PopupMenu.SignalName.IdPressed, popupOptionSelect, (uint)ConnectFlags.Deferred);
            
            AddCustomControl(editorScene);
            AddPropertyEditor("metadata/UmbraHeight", new EditorSpinSliderProperty(-20f, 20f, "m"), false, "Height");
        }
    }
}
#endif