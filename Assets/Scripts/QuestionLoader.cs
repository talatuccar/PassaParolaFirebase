using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniJSON;
using UnityEngine.Networking;

public class QuestionLoader : MonoBehaviour
{

    public GameDataSo gameDataSo;

    [Serializable]
    public class Question
    {
        public string question;
        public string answer;
    }

    Dictionary<int, Question> firebaseQuestions = new Dictionary<int, Question>();
    bool isLoaded = false;

    string firebaseUrl =
        "https://passaparolafirebase-default-rtdb.europe-west1.firebasedatabase.app/passaparola.json";

    void Start()
    {
        StartCoroutine(LoadFromFirebase());
    }

    IEnumerator LoadFromFirebase()
    {
        using (UnityWebRequest req = UnityWebRequest.Get(firebaseUrl))

        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Firebase error: " + req.error);
                yield break;
            }

            string json = req.downloadHandler.text;

            // Firebase JSON -> Dictionary
            var data = Json.Deserialize(json) as Dictionary<string, object>;

            foreach (var item in data)
            {
                string letter = item.Key;

                int index = LetterData.GetIndex(letter);
                if (index == -1)
                    continue;

                var qData = item.Value as Dictionary<string, object>;

                Question q = new Question
                {
                    question = qData["question"].ToString(),
                    answer = qData["answer"].ToString()
                };

                firebaseQuestions[index] = q;
            }


            isLoaded = true;
            Debug.Log("Firebase questions loaded: " + firebaseQuestions.Count);
        }
    }

    void OnEnable()
    {
        GameManager.OnNextAsk += LoadQuestions;
    }

    void OnDisable()
    {
        GameManager.OnNextAsk -= LoadQuestions;
    }

    void LoadQuestions(int soruIndex)
    {
        if (!isLoaded)
        {
            Debug.LogWarning("Questions not loaded yet!");
            return;
        }

        if (!firebaseQuestions.ContainsKey(soruIndex))
        {
            Debug.LogError("Question not found for index: " + soruIndex);
            return;
        }

        var q = firebaseQuestions[soruIndex];
        GameManager.instance.GetQuestionData(q.question, q.answer);
    }

 



















    //    TopicEnum currentQuestionSet;
    //    public GameDataSo gameDataSo;

    //    [Serializable]
    //    public class Question
    //    {
    //        public string question;
    //        public string answer;
    //    }

    //    [Serializable]
    //    public class QuestionList
    //    {
    //        public List<Question> questions;
    //    }
    //    void Start()
    //    {
    //        currentQuestionSet = gameDataSo.QuestionTopic;

    //        LoadQuestions(0);
    //    }
    //    void LoadQuestions(int soruindex)
    //    {
    //        TopicEnum jsonFileName = currentQuestionSet;
    //        TextAsset jsonData = Resources.Load<TextAsset>(jsonFileName.ToString());

    //        if (jsonData != null)
    //        {
    //            QuestionList questionList = JsonUtility.FromJson<QuestionList>(jsonData.ToString());

    //            GameManager.instance.GetQuestionData(questionList.questions[soruindex].question, questionList.questions[soruindex].answer);
    //        }
    //        else
    //        {
    //            Debug.LogError("JSON dosyasý bulunamadý!");
    //        }
    //    }
    //    void OnEnable()
    //    {
    //        GameManager.OnNextAsk += LoadQuestions;
    //    }
    //    void OnDisable()
    //    {
    //        GameManager.OnNextAsk -= LoadQuestions;
    //    }
}
