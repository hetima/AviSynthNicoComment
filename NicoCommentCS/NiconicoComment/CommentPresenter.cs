using System;
using System.Collections.Generic;
using System.Xml;
using NCNicoNico.Comment.Primitive;

namespace NCNicoNico.Comment
{
    public struct CommentLayout
    {
        public Comment Comment;
        public Coordinate Coordinate;
        public bool OverFlow;
    }

    public struct FrameData
    {
        public List<CommentLayout> Layouts;
        public Perm Perm;
        public Vote Vote;
    }

    public class CommentPresenter
    {
        //public class CommentInformation
        //{
        //    public CommentInformation(Comment content)
        //    {
        //        this.Content = content;
        //    }

        //    public Comment Content;
        //    //public double LastY = 0.0;
        //    ////public NCSize Size = new NCSize(0,0);
        //    //public double FontSize = 0.0;
        //}


        public interface ICanvas
        {

            double CanvasWidth { get; }
            double CanvasHeight { get; }
            int RowHeight { get; }
            FontKind FontKind { get; set; }
            double FontSize { get; set; }
            NCSize MeasureText(Comment comment);
            //void FillText(Comment comment, Coordinate coord, bool overFlow);
            Baseline Baseline { get; set; }

            double CurrentSecond { get; }
            double Duration { get; }
        }
        public enum Baseline
        {
            Top,Bottom,Middle
        }
        public enum FontKind
        {
            Default,Mincho,Gothic
        }

        public static double GetYOffsetFromBaseline(Baseline arg)
        {
            switch (arg)
            {
                case Baseline.Top:return 0;
                case Baseline.Middle:return 0.5;
                case Baseline.Bottom:return 1;
                default:return 0;
            }
        }

        //public CommentInformation[] Comments;
        public Comment[] Comments;
        public Vote[] Votes;
        public Perm[] Perms;


        public void Load(string uri, int vposShift, int startPos, int endPos)
        {
            XmlReader xr = XmlReader.Create(uri);
            List<Comment> result = new List<Comment>();
            List<Perm> perms = new List<Perm>();
            List<Vote> votes = new List<Vote>();

            if (xr == null) return;

            int count = 0;
            Comment cmt = null;
            Vote vote = null;
            Perm perm = null;

            while (xr.Read())
            {
                if (xr.ReadToFollowing("chat"))
                {
                    int vpos = int.Parse(xr.GetAttribute("vpos")) - startPos + vposShift;
                    if (vpos < -10 || vpos > endPos + 10) continue;

                    int premium = int.Parse(xr.GetAttribute("premium") ?? "0");
                    string mail = xr.GetAttribute("mail") ?? "";
                    string[] mails = mail.ToLower().Split(' ');
                    cmt = new Comment()
                    {
                        Count = count,
                        //Number = int.Parse(xr.GetAttribute("no")),
                        Vpos = vpos,
                        //Mail = xr.GetAttribute("mail") ?? "",
                        Mails = mails,
                        Text = xr.ReadString()
                    };
                    bool add = true;

                    if (cmt.Text.StartsWith("/hb")) continue;
                    if (cmt.Text.Contains("\n"))// Environment.NewLine
                    {
                        cmt.IsAA = true;
                    }

                    if (premium>=3)
                    {
                        cmt.Text = Comment.StripHtmlTags(cmt.Text);
                        add = false;

                        if (cmt.Text.StartsWith("/clear") || cmt.Text.StartsWith("/cls"))
                        {
                            if (perm != null)
                            {
                                double endTime = cmt.Vpos / 100.0;
                                if (perm.StartTime + 180.0 >= endTime)
                                {
                                    perm.EndTime = endTime;
                                }
                                perm = null;
                            }
                        }
                        else if (cmt.Text.StartsWith("/vote "))
                        {
                            //start showresult stop
                            cmt.IsVote = true;
                            cmt.Text = cmt.Text.Substring(6);
                            if (cmt.Text.StartsWith("start "))
                            {
                                if (vote != null) votes.Add(vote);
                                vote = new Vote();
                                vote.VoteComment = cmt;
                                vote.StartTime = cmt.Vpos / 100.0;
                                vote.ResultTime = (cmt.Vpos / 100.0) + 10.0; //temp
                                vote.EndTime = (cmt.Vpos / 100.0) + 10.0; //temp
                            }
                            else if (cmt.Text.StartsWith("showresult "))
                            {
                                if (vote != null)
                                {
                                    vote.ResultComment = cmt;
                                    vote.ResultTime = (cmt.Vpos / 100.0);
                                    vote.EndTime = (cmt.Vpos / 100.0) + 10.0; //temp
                                }
                            }
                            else if (cmt.Text.StartsWith("stop"))
                            {
                                if (vote != null)
                                {
                                    vote.EndTime = cmt.Vpos / 100.0;
                                    votes.Add(vote);
                                    vote = null;
                                }
                            }
                        }
                        else
                        {
                            if (cmt.Text.StartsWith("/perm "))
                            {
                                cmt.Text = cmt.Text.Substring(6);
                            }
                            cmt.IsPerm = true;
                            perm = new Perm();
                            perm.Comment = cmt;
                            perm.StartTime = cmt.Vpos / 100.0 -3; //ちょっと早めに出す
                            perm.EndTime = perm.StartTime + 12.0; //temp

                            perms.Add(perm);
                        }
                    }
                    if(add) result.Add(cmt);
                    count++;

                }

            }

            if (vote != null)
            {
                votes.Add(vote);
            }

            result.Sort((a, b) =>
            {
                return a.Vpos >= b.Vpos ? 1 : -1;
            });
            //逆順
            votes.Sort((a, b) =>
            {
                return a.StartTime >= b.StartTime ? -1 : 1;
            });
            perms.Sort((a, b) =>
            {
                return a.StartTime >= b.StartTime ? -1 : 1;
            });

            Comments =result.ToArray();
            Perms = perms.ToArray();
            Votes = votes.ToArray();
        }

        public double FontSizeDefault = 30;
        public double FontSizeSmall = 18;
        public double FontSizeBig = 45;

        public double CommentDuration=4.8;

        public FrameData GetFrameData(ICanvas canvas)
        {

            var time = canvas.CurrentSecond;

            List<CommentLayout> layouts = new List<CommentLayout>();
            FrameData result = new FrameData()
            {
                Layouts = layouts,
                Perm = null,
                Vote = null,
            };

            var RangesTop = new List<Range>();
            var RangesBottom = new List<Range>();

            double commentDuration = this.CommentDuration;
            var operatedComments = new List<Comment>();

            foreach (var perm in Perms)
            {
                if(time >= perm.StartTime && time < perm.EndTime)
                {
                    if (perm.Comment.Size.Width <= 0)
                    {
                        perm.Comment.Size = canvas.MeasureText(perm.Comment);
                    }
                    result.Perm = perm;
                    break;
                }
                if(time > perm.EndTime)
                {
                    break;
                }
            }

            foreach (var vote in Votes)
            {

            }
            foreach (var comment in Comments)
            {

                //var vpos = GetActualVpos(comment.Content.Vpos, video.Duration, commentDuration);
                var vpos = comment.Vpos;
                if (time - commentDuration >= vpos / 100.0 || vpos / 100.0 >= time) {
                    if (vpos / 100.0 >= time+10.0) break;
                    continue;
                }
                
                //var mails = comment.Mail.Split(' ');
                if (comment.ContainsCommand("invisible")) { continue; }

                if (comment.FontSize>0){ }
                else if (comment.ContainsCommand("small")) comment.FontSize = FontSizeSmall;
                else if (comment.ContainsCommand("big")) comment.FontSize = FontSizeBig;
                else comment.FontSize = FontSizeDefault;


                var fontSizeActual = comment.FontSize;

                //if (ContainsCommand(mails, "mincho")) canvas.FontKind = FontKind.Mincho;
                //else if (ContainsCommand(mails, "gothic")) canvas.FontKind = FontKind.Gothic;
                //else canvas.FontKind = FontKind.Default;

                var r = new Coordinate();

                var commentSize = new NCSize();
                if (comment.Size.Width>0)
                {
                    commentSize = comment.Size;
                }
                else
                {
                    commentSize = canvas.MeasureText(comment);
                    comment.Size = commentSize;
                }

                //if (ContainsCommand(mails, "ue") || ContainsCommand(mails, "shita"))
                //{
                //    if (commentSize.Width > canvas.CanvasWidth + 1)
                //    {
                //        comment.FontSize *= canvas.CanvasWidth / commentSize.Width;
                //        //comment.FontSize.Relative = Math.Max(comment.FontSize, GetRelativeSize(1, canvas));
                //        canvas.FontSize = comment.FontSize;
                //        fontSizeActual = comment.FontSize;

                //        commentSize = canvas.MeasureText(comment);
                //        comment.Size = commentSize;
                //    }
                //}

                bool overFlow = false;
                
                //if (ContainsCommand(mails, "ue"))
                //{
                //    r = new Coordinate(canvas.CanvasWidth / 2.0 - commentSize.Width / 2.0, Math.Max(0, comment.LastY));
                //    var changed = false;
                //    while(! CheckRanges(RangesTop,new Range(r.Y, r.Y + fontSizeActual)))
                //    {
                //        r.Y += fontSizeActual;
                //        changed = true;
                //    }
                //    if (changed)
                //    {
                //        foreach(var range in RangesTop)
                //        {
                //            if(range.B<r.Y && CheckRanges(RangesTop,new Range(range.B, range.B+fontSizeActual)))
                //                r.Y = range.B;
                //        }
                //    }
                //    canvas.Baseline = Baseline.Top;
                //    comment.LastY = r.Y;
                //    RangesTop.Add(new Range(r.Y, r.Y + fontSizeActual));

                //    overFlow = FixOverflow(ref r.Y, canvas.CanvasHeight,canvas);
                //}
                //else if (ContainsCommand(mails, "shita"))
                //{
                //    r = new Coordinate(canvas.CanvasWidth / 2.0 - commentSize.Width / 2.0, Math.Max(0, comment.LastY));
                //    var changed = false;
                //    while (!CheckRanges(RangesBottom, new Range(r.Y, r.Y + fontSizeActual)))
                //    {
                //        r.Y += fontSizeActual;
                //        changed = true;
                //    }
                //    if (changed)
                //    {
                //        foreach (var range in RangesBottom)
                //        {
                //            if (range.B < r.Y && CheckRanges(RangesBottom, new Range(range.B, range.B+fontSizeActual)))
                //                r.Y = range.B;
                //        }
                //    }
                //    canvas.Baseline = Baseline.Bottom;
                //    comment.LastY = r.Y;
                //    RangesBottom.Add(new Range(r.Y, r.Y + fontSizeActual));

                //    overFlow = FixOverflow(ref r.Y, canvas.CanvasHeight,canvas);

                //    r.Y = canvas.CanvasHeight - r.Y;
                //}
                //else
                {
                    r = GetFromOperated(operatedComments, comment, canvas.Duration, commentDuration, time, canvas, FontSizeDefault);
                    comment.LastY =r.Y;
                    canvas.Baseline = Baseline.Middle;

                    overFlow = FixOverflow(ref r.Y, canvas.CanvasHeight,canvas);

                    r.Y += FontSizeDefault / 8.0;
                    operatedComments.Add(comment);
                }

                if (result.Perm != null)
                {
                    r.Y += canvas.RowHeight;
                }
                layouts.Add(
                    new CommentLayout()
                    {
                        Comment = comment,
                        Coordinate = r,
                        OverFlow = overFlow
                    }
                );
                //canvas.FillText(comment, r, overFlow);
            }
            //canvas.Done();
            return result;
        }

        public bool FixOverflow(ref double y,double canvasHeight,ICanvas canvas)
        {
            bool result = false;
            var modnum = canvasHeight - FontSizeDefault;
            if (y > modnum) { result = true; }
            y %= modnum;
            return result;
        }

        public static double GetPositionX(int vpos,double second,double width,double canvasWidth,double duration)
        {
            var vposSec = vpos / 100.0;
            return (1 - (second - vposSec) / duration) * (canvasWidth + width) - width;
        }

        public bool CheckY(double y1, double y2, double fontHeight)
        {
            var result= y1 - fontHeight * 0.9 < y2 && y2 < y1 + fontHeight * 0.9;
            return result;
        }

        public Coordinate GetFromOperated(List<Comment> operatedComments, Comment currentComment,
            double videoDuration,double commentDuration,double time,ICanvas canvas,double fontHeight)
        {
            var canvasWidth = canvas.CanvasWidth;
            var vpos1 = GetActualVpos(currentComment.Vpos,videoDuration,commentDuration);
            var y = Math.Max(0, currentComment.LastY);
            var firstCollision = true;
            var currentWidth = currentComment.Size.Width;
            var commentHeight = canvas.RowHeight;

            for(int i=0;i< operatedComments.Count;i++)
            {
                var operatedComment = operatedComments[i];
                var vpos2 = GetActualVpos(operatedComment.Vpos, videoDuration, commentDuration);
                var operatedWidth = operatedComment.Size.Width;

                var a = GetPositionX(vpos1, vpos2/100.0 + commentDuration, currentWidth, canvasWidth, commentDuration);
                var b = GetPositionX(vpos2, vpos1 / 100.0, operatedWidth, canvasWidth, commentDuration) + operatedWidth;
                var c = GetPositionX(vpos2, vpos1 / 100.0 + commentDuration, operatedWidth, canvasWidth, commentDuration);
                var d = GetPositionX(vpos1, vpos2 / 100.0, currentWidth, canvasWidth, commentDuration) + currentWidth;

                if(
                    CheckY(y,operatedComment.LastY,fontHeight)&&
                    (!(((b<canvasWidth)&&(0<a)&&(vpos2<=vpos1))||((c<canvasWidth)&&(0<d)&&(vpos1<vpos2))))
                    )
                {
                    i = -1;
                    if (firstCollision)
                    {
                        firstCollision = false;
                        y = 0;
                    }
                    else
                    {
                        y += commentHeight; //fontHeight.GetActual(canvas);
                    }
                }
            }
            var x = GetPositionX(vpos1, time, currentWidth, canvasWidth, commentDuration);
            return new Coordinate(x, y);
        }

        private bool CheckRanges(List<Range> ranges,Range range)
        {
            if (ranges == null || ranges.Count == 0) return true;
            foreach(var item in ranges)
            {
                if (!range.Check(item,false,true)) { return false; }
            }
            return true;
        }

        private int GetActualVpos(int vpos,double Duration,double CommentDuration)
        {
            return (int)Math.Min(vpos, Duration * 100 - CommentDuration * 100 * 0.8);
        }

    }
}
