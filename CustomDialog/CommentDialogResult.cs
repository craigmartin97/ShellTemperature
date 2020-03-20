namespace CustomDialog.Annotations
{
    public class CommentDialogResult
    {
        public string Comment { get; set; }
        public bool Delete { get; set; }

        public CommentDialogResult(string comment, bool delete)
        {
            Comment = comment;
            Delete = delete;
        }
    }
}