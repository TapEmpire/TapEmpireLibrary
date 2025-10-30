#import <Foundation/Foundation.h>

extern "C" bool te_isSandboxReceipt() {
    NSURL *url = [[NSBundle mainBundle] appStoreReceiptURL];
    return [[url lastPathComponent] isEqualToString:@"sandboxReceipt"];
}
