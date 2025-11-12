import UIKit
import SwiftUI

private var host: UIViewController?

struct FullScreenView: View {
    let title: String
    let message: String

    var body: some View {
        ZStack {
            Color.green.opacity(0.2).compatIgnoresSafeArea() // .ignoresSafeArea()
            VStack(spacing: 16) {
                Text(title)
                    .font(.system(size: 24, weight: .bold))
                    .foregroundColor(.white)
                Text(message)
                    .font(.system(size: 16))
                    .foregroundColor(.white)
                    .multilineTextAlignment(.center)
                Button("Close") { /*NativeFullScreen.dismiss()*/ }
                    .font(.system(size: 18, weight: .bold))
                    .padding(.horizontal, 20).padding(.vertical, 10)
                    .background(Color.white.opacity(0.2)).cornerRadius(12)
            }
            .padding(40)
            .background(Color.black.opacity(0.5)).cornerRadius(16)
            .padding(.horizontal, 24)
            .frame(maxWidth: 800, maxHeight: 600, alignment: .center)
        }
        .statusBarHidden(true)
    }
}

@_cdecl("nfs_show")
public func nfs_show(_ cTitle: UnsafePointer<CChar>?, _ cMessage: UnsafePointer<CChar>?) {
    DispatchQueue.main.async {
        guard host == nil else { return }
        let title = "Title M" // cTitle.flatMap { String(cString: $0) } ?? ""
        let message = "Message" // cMessage.flatMap { String(cString: $0) } ?? ""
        let vc = UIHostingController(rootView: FullScreenView(title: title, message: message))
        vc.modalPresentationStyle = .fullScreen
        if let top = topVC() {
            host = vc
            top.present(vc, animated: true, completion: nil)
        }
    }
}

@_cdecl("nfs_dismiss")
public func nfs_dismiss() {
    DispatchQueue.main.async {
        host?.dismiss(animated: true, completion: { host = nil })
    }
}

private func topVC() -> UIViewController? {
    var root = UIApplication.shared.keyWindow?.rootViewController
    if root == nil {
        for scene in UIApplication.shared.connectedScenes {
            if let ws = scene as? UIWindowScene, scene.activationState == .foregroundActive {
                root = ws.windows.first(where: { $0.isKeyWindow })?.rootViewController
                break
            }
        }
    }
    while let presented = root?.presentedViewController { root = presented }
    return root
}

private extension UIApplication {
    var keyWindow: UIWindow? {
        if #available(iOS 13.0, *) {
            return self.connectedScenes
                .compactMap { $0 as? UIWindowScene }
                .flatMap { $0.windows }
                .first { $0.isKeyWindow }
        } else {
            return self.keyWindow
        }
    }
}

extension View {
    @ViewBuilder
    func compatIgnoresSafeArea() -> some View {
        if #available(iOS 14.0, *) {
            self.ignoresSafeArea()                       // iOS 14+
        } else {
            self.edgesIgnoringSafeArea(.all)             // iOS 13 fallback
        }
    }
}
