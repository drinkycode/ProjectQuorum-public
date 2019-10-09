using ProjectQuorum.Data;
using ProjectQuorum.Data.Variables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectQuorum.Managers
{

    public class ConnectionMgr : MonoBehaviour
    {
        // This class is excluded for security purposes. Please contact the program manager for further information.
    }

    [Serializable]
    public class LoginData
    {

        public string id;
        public string status;
        public string token;

    }

    [Serializable]
	public class AccountData
	{

		public string id;
		public string username;
		public string email;
		public string first_name;
		public string last_name;
		public string token_expire;
		public string role;
        public string score;

        public void Clear()
		{
			id = string.Empty;
			username = string.Empty;
			email = string.Empty;
			first_name = string.Empty;
			last_name = string.Empty;
			token_expire = string.Empty;
			role = string.Empty;
            score = string.Empty;
        }

	}

    [Serializable]
    public class AccountsObject
    {
        public AccountData[] accounts;
    }

    [Serializable]
    public class ProjectWorldData
    {

        public string id;
        public string user_id;
        public string name;
        public string description;
        public string status;
        public string tool_type;
        public string approved;
        public string cover_image;
        public string created_at;
        public string updated_at;
        public WorldLevelData[] levels;

    }

    [Serializable]
    public class WorldLevelData
    {

        public string id;
        public string project_id;
        public string name;
        public string is_mystery;
        public string image_url;
        public string photo_inverted;
        public string segment_text_data;
        public string description;
        public string ground_truths_photo;
        public string ground_truths_text_data;
        public string best_time;
        public string best_time_user_id;
        public string num_attempts;
        public string created_at;
        public string update_at;

    }

}