/* ----------------------------------------------------------------------------
 * This file was automatically generated by SWIG (http://www.swig.org).
 * Version 2.0.4
 *
 * Do not make changes to this file unless you know what you are doing--modify
 * the SWIG interface file instead.
 * ----------------------------------------------------------------------------- */

namespace CrankcaseAudio.Wrappers {

using System;
using System.Runtime.InteropServices;

public partial class FloatArray : IDisposable {
  private HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal FloatArray(IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new HandleRef(this, cPtr);
  }

  internal static HandleRef getCPtr(FloatArray obj) {
    return (obj == null) ? new HandleRef(null, IntPtr.Zero) : obj.swigCPtr;
  }

  ~FloatArray() {
    Dispose();
  }

  public virtual void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          CrankcaseAudioPINVOKE.delete_FloatArray(swigCPtr);
        }
        swigCPtr = new HandleRef(null, IntPtr.Zero);
      }
      GC.SuppressFinalize(this);
    }
  }

  public FloatArray(int nelements) : this(CrankcaseAudioPINVOKE.new_FloatArray(nelements), true) {
  }

  public float getitem(int index) {
    float ret = CrankcaseAudioPINVOKE.FloatArray_getitem(swigCPtr, index);
    return ret;
  }

  public void setitem(int index, float value) {
    CrankcaseAudioPINVOKE.FloatArray_setitem(swigCPtr, index, value);
  }

  public SWIGTYPE_p_float cast() {
    IntPtr cPtr = CrankcaseAudioPINVOKE.FloatArray_cast(swigCPtr);
    SWIGTYPE_p_float ret = (cPtr == IntPtr.Zero) ? null : new SWIGTYPE_p_float(cPtr, false);
    return ret;
  }

  public static FloatArray frompointer(SWIGTYPE_p_float t) {
    IntPtr cPtr = CrankcaseAudioPINVOKE.FloatArray_frompointer(SWIGTYPE_p_float.getCPtr(t));
    FloatArray ret = (cPtr == IntPtr.Zero) ? null : new FloatArray(cPtr, false);
    return ret;
  }

}

}
