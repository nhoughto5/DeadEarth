using UnityEngine;

[ExecuteInEditMode]
public class CameraBloodEffect : MonoBehaviour {
	[SerializeField] private bool _autoFade = true;
	[SerializeField] private float _bloodAmount = 0.5f;
	[SerializeField] private Texture2D _bloodNormalMap = null;
	[SerializeField] private Texture2D _bloodTexture = null;
	[SerializeField] private float _distortion = 1.0f;
	[SerializeField] private float _fadeSpeed = 0.05f;
	private Material _material;
	[SerializeField] private float _minBloodAmount;
	[SerializeField] private Shader _shader = null;
	public bool autoFade { get { return _autoFade; } set { _autoFade = value; } }
	public float bloodAmount { get { return _bloodAmount; } set { _bloodAmount = value; } }
	public float fadeSpeed { get { return _fadeSpeed; } set { _fadeSpeed = value; } }
	public float minBloodAmount { get { return _minBloodAmount; } set { _minBloodAmount = value; } }

	private void OnRenderImage(RenderTexture src, RenderTexture dest) {
		if (_shader == null) { return; }

		if (_material == null) { _material = new Material(_shader); }
		if (_material == null) { return; }

		//Send Data into shader
		if (_bloodTexture != null) { _material.SetTexture("_BloodTex", _bloodTexture); }
		if (_bloodNormalMap != null) { _material.SetTexture("_BloodBump", _bloodNormalMap); }
		_material.SetFloat("_Distortion", _distortion);
		_material.SetFloat("_BloodAmount", _bloodAmount);

		Graphics.Blit(src, dest, _material);
	}

	private void Update() {
		if (_autoFade) {
			_bloodAmount -= _fadeSpeed * Time.deltaTime;
			_bloodAmount = Mathf.Max(_bloodAmount, _minBloodAmount);
		}
	}
}