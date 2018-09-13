﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CustomTranslatorSampleCode.Controllers
{
    public class ModelController : Controller
    {
        // GET: Model
        public async Task<ActionResult> Index(int? id)
        {
            string token_header = null;

            if (Session["token_header"] != null)
            {
                token_header = Session["token_header"].ToString();
            }
            CustomTranslatorAPIClient clientapp = new CustomTranslatorAPIClient();

            if (id == null)
            {
                return View();
            }

            long modelid = (long) id;
            string result = await clientapp.GetModel(modelid, token_header);

            TrainingModel m = getTrainingDetail(result);

            Response.Write("<br />Model Id: " + m.id);
            Response.Write("<br />Model Name: " + m.name);
            Response.Write("<br />Model Status: " + m.modelStatus);
            Response.Write("<br/>");

            foreach (Document d in m.documents)
            {
                Response.Write("<br/>Document Name: " + d.documentInfo.name);
                Response.Write("<br/>Aligned Sentence Count: " + d.processedDocumentInfo.alignedSentenceCount);
                Response.Write("<br/>Used Sentence Count: " + d.processedDocumentInfo.usedSentenceCount);


                int i = 1;
                foreach (FileInDocument f in d.documentInfo.files)
                {
                    Response.Write("<br/>File " + i + " : " + f.fileName);
                    Response.Write("<br/>Language: " + f.language.displayName);
                    i++;
                }
            }
            Response.Write("<br/>");
            Response.Write("<br />Bleu Score: " + m.bleuScoreCIPunctuated);
            Response.Write("<br />Training Sentence Count: " + m.trainingSentenceCount);
            Response.Write("<br />Tuning Sentence Count: " + m.tuningSentenceCount);
            Response.Write("<br />Testing Sentence Count: " + m.testingSentenceCount);

            return View();
        }


        public async Task<ActionResult> Create()
        {
            string token_header = null;

            if (Session["token_header"] != null)
            {
                token_header = Session["token_header"].ToString();
            }

            CustomTranslatorAPIClient clientapp = new CustomTranslatorAPIClient();

            ModelCreateRequest model = new ModelCreateRequest(); // Create new object for Model and add values
            model.name = "EN DA Model 2 "; // Enter model name
            model.projectId = "f923bde7-c36d-4570-9a67-a8e835bf2cbc"; // Enter project id
            model.documentIds = new List<int>();
            model.documentIds.Add(62107111); // Add multiple documents using DocumentID. DocumentID is int.
            model.isTuningAuto = true; // Enter if tuning set will be set to auto. values = true, false
            model.isTestingAuto = true; // Enter if testing set will be set to auto. values = true, false
            model.isAutoDeploy = false; // Enter if this model will be automatically deployed. values = true, false
            model.autoDeployThreshold = 0; // Enter the value of auto deploy threshold value

            string result = await clientapp.GetProject(model.projectId, token_header);
            Project project = getProjectDetail(result);

            result = await clientapp.CreateModel(token_header, model);
            string[] resultarray = result.Split('/');

            if (resultarray.Length > 1)
            {
                string newtrainingid = resultarray[resultarray.Length - 1];
                Response.Write("<br/>New Model Created");
                Response.Write("<br/>Model Id: " + newtrainingid);
                Response.Write("<br/>Model Name: " + model.name);
                Response.Write("<br/>Project Name: " + project.name);
                Response.Write("<br/>Language Pair: " + project.languagePair.sourceLanguage.displayName + " to " + project.languagePair.targetLanguage.displayName);
                foreach (int fileid in model.documentIds)
                {
                    result = await clientapp.GetDocument(fileid, token_header);
                    DocumentInfo doc = getDocumentDetail(result);
                    Response.Write("<br/>Document : " + doc.name);
                    int i = 1;
                    foreach (FileInDocument f in doc.files)
                    {
                        Response.Write("<br/>File " + i + " :");
                        Response.Write("<br/>File Name: " + f.fileName);
                        Response.Write("<br/>Language: " + f.language.displayName);
                        i++;
                    }
                }
            }
            else
            {
                Response.Write("<br/>Could not create project: " + result);
            }

            return View();
        }

        public async Task<ActionResult> Deploy(int? id)
        {
            string token_header = null;

            if (Session["token_header"] != null)
            {
                token_header = Session["token_header"].ToString();
            }
            if (id == null)
            {
                return View();
            }

            long modelid = (long)id;
            CustomTranslatorAPIClient clientapp = new CustomTranslatorAPIClient();
            string result = await clientapp.CreateModelDeploymentRequest(modelid, "deploy", token_header);
            Response.Write(result);
            return View();
        }

            Project getProjectDetail(string result)
        {
            Project project = JsonConvert.DeserializeObject<Project>(result);
            return project;
        }

        DocumentInfo getDocumentDetail(string result)
        {
            DocumentInfo doc = JsonConvert.DeserializeObject<DocumentInfo>(result);
            return doc;
        }

        TrainingModel getTrainingDetail(string result)
        {
            TrainingModel model = JsonConvert.DeserializeObject<TrainingModel>(result);
            return model;
        }
    }
}