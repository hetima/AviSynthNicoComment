#include "pch.h"
#include <windows.h>

#include "AviSynthNicoComment.h"
#include <avisynth.h>

#pragma managed
#include <gcroot.h>
using namespace NicoCommentCS;
using namespace System;
using namespace System::Drawing;

namespace AviSynthNicoComment {

}



class CommentWrapper : public GenericVideoFilter {
public:
    CommentWrapper(AVSValue args, IScriptEnvironment* env);
    void copyImage(PVideoFrame vf, gcroot<Bitmap^> canvas);
    PVideoFrame __stdcall GetFrame(int n, IScriptEnvironment* env);
private:
    gcroot<Filter^> cs;
};


CommentWrapper::CommentWrapper(AVSValue args, IScriptEnvironment* env) : GenericVideoFilter(args[0].AsClip()) {
    vi.pixel_type = VideoInfo::CS_BGR32;
    const char* filePath = args[1].AsString();
    int rowHeight = args[2].AsInt();
    if (rowHeight <= 0) {
        rowHeight = 60;
    }
    double duration = (double)vi.num_frames * (double)vi.fps_denominator / (double)vi.fps_numerator;
    int vposShift = (int)(args[3].AsFloat()*100) + (int)(args[4].AsFloat() * 100);
    int start = 0;
    int end = start + (int)(duration * 100);

    cs = gcnew NicoCommentCS::Filter((double)vi.width, (double)vi.height, duration, rowHeight);
    cs->LoadFilePathWithCString((unsigned char*)filePath, vposShift, start, end);
}


PVideoFrame __stdcall CommentWrapper::GetFrame(int n, IScriptEnvironment* env) {

    //PVideoFrame src = child->GetFrame(n, env);
    double position = (double)n * (double)vi.fps_denominator / (double)vi.fps_numerator;
    PVideoFrame dst = env->NewVideoFrame(vi);
    unsigned char* dstp;

    dstp = dst->GetWritePtr();
    cs->DrawFrame(dstp, position);

    return dst;
}



AVSValue __cdecl createNicoComment(AVSValue args, void* user_data, IScriptEnvironment* env)
{
    //CommentPresenter cp = new CommentPresenter();
    //avisynth_nico_comment::add(1, 2);
    return new CommentWrapper(args, env);
    // return NULL;
}

const AVS_Linkage* AVS_linkage = 0;


extern "C" __declspec(dllexport) const char* __stdcall AvisynthPluginInit3(IScriptEnvironment * env, const AVS_Linkage* const vectors)
{
    AVS_linkage = vectors;
    env->AddFunction("NicoComment", "[clip]c[file]s[row]i[shift]f[shiftb]f", createNicoComment, 0);
    return "NicoComment plugin";
}

