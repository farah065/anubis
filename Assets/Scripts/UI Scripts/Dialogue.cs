using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Dialogue : Singleton<Dialogue>
{
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _dialogueText;
    [SerializeField] private Button _continueButton;
    [SerializeField] private GameObject _dialogueBox;
    [SerializeField] private string[] _sentences;
    [SerializeField] private Image _featherImage;

    private int _currentSentenceIndex = 0;

    private bool _canCoroutineRun = true;

    public void StartDialogue()
    {
        _nameText.text = "???";
        _dialogueBox.SetActive(true);
        StartCoroutine(DisplayDialogue(_sentences[_currentSentenceIndex]));
    }

    private void HideDialogue()
    {
        _dialogueBox.SetActive(false);
    }

    public IEnumerator DisplayDialogue(string message)
    {
        _dialogueText.text = "";
        _continueButton.gameObject.SetActive(false);
        foreach (char letter in message.ToCharArray())
        {
            _dialogueText.text += letter;
            yield return new WaitForSeconds(0.025f);
        }
        _continueButton.gameObject.SetActive(true);
    }

    public void OnContinueButtonPressed()
    {
        if (_currentSentenceIndex == 2)
        {
            _nameText.text = "Ma'at";
        }
        else if (_currentSentenceIndex == 6)
        {
            // tween the feather image to move up and fade in
            _featherImage.transform.DOMoveY(_featherImage.transform.position.y + 100f, 1f).SetEase(Ease.OutQuad);
            _featherImage.DOFade(1f, 1f);
        }
        else if (_currentSentenceIndex == 7)
        {
            // tween the feather image to move down and fade out
            _featherImage.transform.DOMoveY(_featherImage.transform.position.y - 100f, 1f).SetEase(Ease.InQuad);
            _featherImage.DOFade(0f, 1f).OnComplete(() => _featherImage.gameObject.SetActive(false));
        }

        _currentSentenceIndex++;
        if (_currentSentenceIndex < _sentences.Length)
        {
            StartCoroutine(DisplayDialogue(_sentences[_currentSentenceIndex]));
        }
        else
        {
            HideDialogue();
            _currentSentenceIndex = 0;
            StartCoroutine(TeleportPlayerBack());
        }
    }

    private IEnumerator TeleportPlayerBack()
    {
        yield return new WaitForSeconds(2f);
        GameManager.Instance.ReturnToCooldownRoom();
    }
}
