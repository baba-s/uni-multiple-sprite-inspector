using System;
using UnityEditor;
using UnityEngine;

[CustomEditor( typeof( Sprite ) )]
public sealed class UniMultipleSpriteInspector : Editor
{
	private const int BORDER_SIZE = 2;

	private Texture2D		m_mainAsset		;
	private TextureImporter	m_importer		;
	private Texture2D		m_borderTexture	;

	private Vector4 m_borderInit;
	private Vector4 m_border;

	private Texture2D mainAsset
	{
		get
		{
			if ( m_mainAsset == null )
			{
				var path = AssetDatabase.GetAssetPath( target );
				m_mainAsset = AssetDatabase.LoadMainAssetAtPath( path ) as Texture2D;
			}
			return m_mainAsset;
		}
	}

	private TextureImporter importer
	{
		get
		{
			if ( m_importer == null )
			{
				var path = AssetDatabase.GetAssetPath( target );
				m_importer = AssetImporter.GetAtPath( path ) as TextureImporter;
			}
			return m_importer;
		}
	}

	private Texture2D borderTexture
	{
		get
		{
			if ( m_borderTexture == null )
			{
				var path = "Assets/UniMultipleSpriteInspector/Editor/border.png";
				m_borderTexture = AssetDatabase.LoadAssetAtPath<Texture2D>( path );
			}
			return m_borderTexture;
		}
	}

    public override bool HasPreviewGUI() => true;

	private void OnEnable()
	{
        var sprite			= target as Sprite;
		var spritesheet		= importer.spritesheet;
		var index			= Array.FindIndex( spritesheet, c => c.name == sprite.name );
		var spriteMetaData	= spritesheet[ index ];
		var rect			= spriteMetaData.rect;
		var border			= spriteMetaData.border;


		m_border = border;
		m_borderInit = border;
	}

	public override void OnInspectorGUI()
	{
        var sprite			= target as Sprite;
		var spritesheet		= importer.spritesheet;
		var index			= Array.FindIndex( spritesheet, c => c.name == sprite.name );
		var spriteMetaData	= spritesheet[ index ];
		var rect			= spriteMetaData.rect;
		var width			= rect.width;
		var height			= rect.height;
		var oldBorder		= m_border;

		var enabled = GUI.enabled;
		GUI.enabled = false;

		EditorGUILayout.RectField( "Rect", rect );

		GUI.enabled = enabled;

		m_border = EditorGUILayout.Vector4Field( "Border", m_border );

		if ( oldBorder.x != m_border.x )
		{
			m_border.x = ( int )Mathf.Clamp( m_border.x, 0, width - m_border.z );
		}
		if ( oldBorder.y != m_border.y )
		{
			m_border.y = ( int )Mathf.Clamp( m_border.y, 0, height - m_border.w );
		}
		if ( oldBorder.z != m_border.z )
		{
			m_border.z = ( int )Mathf.Clamp( m_border.z, 0, width  - m_border.x );
		}
		if ( oldBorder.w != m_border.w )
		{
			m_border.w = ( int )Mathf.Clamp( m_border.w, 0, height - m_border.y );
		}

		GUILayout.BeginHorizontal();

		if ( GUILayout.Button( "Revert" ) )
		{
			m_border = m_borderInit;
		}
		if ( GUILayout.Button( "Apply" ) )
		{
			m_border.x = Mathf.Round( m_border.x );
			m_border.y = Mathf.Round( m_border.y );
			m_border.z = Mathf.Round( m_border.z );
			m_border.w = Mathf.Round( m_border.w );

			spriteMetaData.border = m_border;
			spritesheet[ index ] = spriteMetaData;

			importer.spritesheet = spritesheet;

			EditorUtility.SetDirty( importer );
			importer.SaveAndReimport();
		}

		GUILayout.EndHorizontal();
	}

	public override void OnPreviewGUI( Rect r, GUIStyle background )
    {
        var position		= r;
        var sprite			= target as Sprite;
		var texture			= sprite.texture;
		var textureRect		= sprite.textureRect;
        var fullSize		= new Vector2( texture.width, texture.height );
        var size			= new Vector2( textureRect.width, textureRect.height );
		var border			= m_border;

        var coords = textureRect;
        coords.x		/= fullSize.x;
        coords.width	/= fullSize.x;
        coords.y		/= fullSize.y;
        coords.height	/= fullSize.y;

        Vector2 ratio;
        ratio.x = position.width / size.x;
        ratio.y = position.height / size.y;
        float minRatio = Mathf.Min( ratio.x, ratio.y );

        var center = position.center;
        position.width	= size.x * minRatio;
        position.height	= size.y * minRatio;
        position.center	= center;

        GUI.DrawTextureWithTexCoords( position, texture, coords );

		if ( 0 < border.x )
		{
			var rate	= border.x / size.x;
			var pos		= new Rect
			(
				x		: position.x + position.width * rate,
				y		: position.y,
				width	: BORDER_SIZE,
				height	: position.height
			);

			GUI.DrawTexture( pos, borderTexture );
		}

		if ( 0 < border.w )
		{
			var rate	= border.w / size.y;
			var pos		= new Rect
			(
				x		: position.x,
				y		: position.y + position.height * rate,
				width	: position.width,
				height	: BORDER_SIZE
			);

			GUI.DrawTexture( pos, borderTexture );
		}

		if ( 0 < border.z )
		{
			var rate	= border.z / size.x;
			var pos		= new Rect
			(
				x		: position.x + position.width - position.width * rate,
				y		: position.y,
				width	: BORDER_SIZE,
				height	: position.height
			);

			GUI.DrawTexture( pos, borderTexture );
		}

		if ( 0 < border.y )
		{
			var rate	= border.y / size.y;
			var pos		= new Rect
			(
				x		: position.x,
				y		: position.y + position.height - position.height * rate,
				width	: position.width,
				height	: BORDER_SIZE
			);

			GUI.DrawTexture( pos, borderTexture );
		}
    }
}